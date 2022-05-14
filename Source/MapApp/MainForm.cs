using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections.Generic;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;

namespace MapApp
{
    public sealed partial class MainForm : Form
    {
        string strPath = System.Environment.CurrentDirectory + @"\map\map.mxd"; //地图路径
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;
        #endregion

        #region class constructor
        IActiveView m_ipActiveView = null;
        public MainForm()
        {
            InitializeComponent();
            m_ipActiveView = axMapControl1.ActiveView;
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //get the MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;

            //加载地图
            //string sPath = System.IO.Path.Combine(Application.StartupPath, @"test\map.mxd");
            axMapControl1.LoadMxFile(strPath);
            axMapControl1.Refresh();
        }

        #region Main Menu event handlers
        private void menuNewDoc_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuOpenDoc_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuSaveDoc_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (m_mapControl.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocumentClass();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("Map document is read only!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)m_mapControl.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            //execute SaveAs Document command
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuExitApp_Click(object sender, EventArgs e)
        {
            //exit the application
            Application.Exit();
        }
        #endregion

        //listen to MapReplaced evant in order to update the statusbar and the Save menu
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            //get the current document name from the MapControl
            m_mapDocumentName = m_mapControl.DocumentFilename;

            //if there is no MapDocument, diable the Save menu and clear the statusbar
            if (m_mapDocumentName == string.Empty)
            {
                statusBarXY.Text = string.Empty;
            }
            else
            {
                //enable the Save manu and write the doc name to the statusbar
                statusBarXY.Text = System.IO.Path.GetFileName(m_mapDocumentName);
            }

            //获得mapcontrol1的map对象
            IMap map = axMapControl1.Map;

            //遍历map中的图层，都添加到mapcontrol2中
            for (int i = 0; i < map.LayerCount; i++)
            {
                axMapControl2.AddLayer(map.get_Layer(i));
            }
            axMapControl2.Refresh();
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }

        private ILayer GetLayer(string LayerName)
        {
            for (int i = 0; i < axMapControl1.Map.LayerCount; i++)
            {
                ILayer pLayer = axMapControl1.Map.get_Layer(i);
                if (pLayer.Name.Equals(LayerName))
                {
                    return pLayer;
                }
            }
            return null;

        }
   
        public string GetWherePoint(ILayer layer, string KeyWords, string FieldName)
        {
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            //获取featureLayer的featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = "1=1";
            featureCusor = featureClass.Search(queryFilter, true);
            string nearName = string.Empty;

            string temp = string.Empty;
            int findex = -1;
            //查找出所有点并计算距离
            while ((feature = featureCusor.NextFeature()) != null)
            {
                bool bContain = false;
                for (int i = 0; i < feature.Fields.FieldCount; i++)
                {
                    if (feature.get_Value(i) != null && feature.get_Value(i).ToString().Contains(KeyWords))
                    {
                        bContain = true;
                        axMapControl1.FlashShape(feature.Shape);
                        break;
                    }
                }
                if (bContain)
                {
                    //包含关键字
                    if (findex == -1)
                    {
                        findex = feature.Fields.FindField(FieldName);
                    }
                    temp += feature.get_Value(findex) + ",";
                }
            }
            return temp.TrimEnd(',');
        }

        public string GetFeature(ILayer layer, string KeyWords, string FieldName)
        {
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            //获取featureLayer的featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = "1=1";
            featureCusor = featureClass.Search(queryFilter, true);
            string nearName = string.Empty;

            string temp = string.Empty;

            //查找出所有点并计算距离
            while ((feature = featureCusor.NextFeature()) != null)
            {
                int index = feature.Fields.FindField(FieldName);
                if (KeyWords.Equals(feature.get_Value(index)))
                {
                    axMapControl1.FlashShape(feature.Shape);
                }
            }
            return temp.TrimEnd(',');
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            axMapControl2.Location = new System.Drawing.Point(this.Width - axMapControl2.Width-30, this.Height - axMapControl2.Height-70);
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //得到新范围
            IEnvelope2 envelope = e.newEnvelope as IEnvelope2;
            //生成一个矩形要素
            IRectangleElement rectangleelement = new RectangleElementClass();
            //得到一个要素并将矩形要素传入
            IElement element = rectangleelement as IElement;
            //将新范围赋值给要素的几何外形
            element.Geometry = envelope;
            //得到RGBColor对象
            IRgbColor color = new RgbColorClass();
            //设置Color的RGB值
            color.RGB = 00000255;
            //设置透明度
            color.Transparency = 255;
            //产生一个线状符号
            ILineSymbol lineSymbool = new SimpleLineSymbolClass();
            //设置线状符号的属性
            lineSymbool.Width = 1;
            lineSymbool.Color = color;
            //设置颜色属性
            color.RGB = 255;
            color.Transparency = 0;
            //生成一个填充符号用来填充矩形区域
            IFillSymbol fillSymbol = new SimpleFillSymbolClass();
            //设置填充符号的属性
            //填充色
            fillSymbol.Color = color;
            //外轮廓
            fillSymbol.Outline = lineSymbool;
            //得到一个填充图形要素，AE帮助中对IFillShapElement的解释为
            /*IFillShapeElement is a generic interface implemented by all 2d elements (CircleElement,
             * EllipseElement, PolygonElement, and RectangleElement).
             * Use this interface when you want to retrieve or set the fill symbol being used by one 
             * of the fill shape elements.
            */
            IFillShapeElement fillShapElement = element as IFillShapeElement;
            //将填充符号传入Symbol中
            fillShapElement.Symbol = fillSymbol;
            //最后将填充图形要素赋值给要素类
            element = fillShapElement as IElement;

            //得到图像容器,并将mapcontrol2的map传入
            IGraphicsContainer graphicsContainer = axMapControl2.Map as IGraphicsContainer;
            //得到一个活动视图并将graphicscontainer传入
            IActiveView activeView = graphicsContainer as IActiveView;

            //在绘制新的矩形框前，清楚Map对象中的任何图形元素
            graphicsContainer.DeleteAllElements();

            //将要素添加到图像容器中,并将其置于顶层0
            graphicsContainer.AddElement(element, 0);
            //将图像容器装入活动视图中并刷新
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void menuSearch_Click(object sender, EventArgs e)
        {
            Search frmSearch = new Search();
            if (frmSearch.ShowDialog() == DialogResult.OK)
            {
                this.Refresh();
                //查询
                try
                {
                    ILayer layer=GetLayer("zongdi");
                    bool bFind = false;
                    SearchFeature(layer, frmSearch.SelectItem, frmSearch.SelectType, out bFind);
                    if (!bFind)
                    {
                        MessageBox.Show("未查找到符合条件的宗地！");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("查询出错：" + ex.Message);
                }
            }
        }

        public void SearchFeature(ILayer layer, string KeyWords, string FieldName, out bool bFind)
        {
            bFind = false;
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            //获取featureLayer的featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = string.Format("{0}='{1}'", FieldName, KeyWords);
            featureCusor = featureClass.Search(queryFilter, true);
            //查找出所有点并计算距离
            while ((feature = featureCusor.NextFeature()) != null)
            {
                bFind = true;
                axMapControl1.ActiveView.Extent = feature.Shape.Envelope;
                axMapControl1.Refresh();
                axMapControl1.FlashShape(feature.Shape);
                //bool bContain = false;
                //for (int i = 0; i < feature.Fields.FieldCount; i++)
                //{
                //    if (feature.get_Value(i) != null && feature.get_Value(i).ToString().Contains(KeyWords))
                //    {
                //        bContain = true;
                //        break;
                //    }
                //}
                //if (bContain)
                //{
                //    axMapControl1.ActiveView.Extent = feature.Shape.Envelope;
                //    axMapControl1.Refresh();
                //    axMapControl1.FlashShape(feature.Shape);
                //}
            }
        }

        private void menuPrint_Click(object sender, EventArgs e)
        {
            SelectZongDi frmSearch = new SelectZongDi();
            if (frmSearch.ShowDialog() == DialogResult.OK)
            {
                this.Refresh();
                //查询
                try
                {
                    ILayer layer = GetLayer("zongdi");
                    bool bFind = false;
                    PrintZongdiFeature(layer, frmSearch.SelectItem, out bFind);
                    if (!bFind)
                    {
                        MessageBox.Show("未查找到符合条件的宗地！");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("查询出错：" + ex.Message);
                }
            }
        }

        public void PrintZongdiFeature(ILayer layer, string KeyWords, out bool bFind)
        {
            bFind = false;
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            //获取featureLayer的featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = string.Format("宗地编号='{0}'",  KeyWords);
            featureCusor = featureClass.Search(queryFilter, true);
            //查找出所有点并计算距离
            while ((feature = featureCusor.NextFeature()) != null)
            {
                bFind = true;
                axMapControl1.ActiveView.Extent = feature.Shape.Envelope;
                axMapControl1.Refresh();
                axMapControl1.FlashShape(feature.Shape);
                axMapControl1.Refresh();
               
                 Bitmap bitmap=new Bitmap(axMapControl1.Width,axMapControl1.Height);
                (axMapControl1 as Control).DrawToBitmap(bitmap,new Rectangle(0,0,axMapControl1.Width,axMapControl1.Height));

                PrintZongDi frmPrintZongDi = new PrintZongDi();
                frmPrintZongDi.宗地编号 = KeyWords;
                frmPrintZongDi.Text = "宗地编号：" + KeyWords;
                int findex = feature.Fields.FindField("权利人");
                frmPrintZongDi.使用人 = feature.get_Value(findex).ToString();
                findex = feature.Fields.FindField("图号");
                frmPrintZongDi.图号 = feature.get_Value(findex).ToString();
                findex = feature.Fields.FindField("坐落");
                frmPrintZongDi.坐落 = feature.get_Value(findex).ToString();
                findex = feature.Fields.FindField("使用权面积");
                frmPrintZongDi.使用面积 = feature.get_Value(findex).ToString();
                frmPrintZongDi.ShowDialog();
            }
        }
    }
}