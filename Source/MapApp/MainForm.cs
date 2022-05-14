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
        string strPath = System.Environment.CurrentDirectory + @"\map\map.mxd"; //��ͼ·��
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

            //���ص�ͼ
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

            //���mapcontrol1��map����
            IMap map = axMapControl1.Map;

            //����map�е�ͼ�㣬����ӵ�mapcontrol2��
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
            //��ȡfeatureLayer��featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = "1=1";
            featureCusor = featureClass.Search(queryFilter, true);
            string nearName = string.Empty;

            string temp = string.Empty;
            int findex = -1;
            //���ҳ����е㲢�������
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
                    //�����ؼ���
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
            //��ȡfeatureLayer��featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = "1=1";
            featureCusor = featureClass.Search(queryFilter, true);
            string nearName = string.Empty;

            string temp = string.Empty;

            //���ҳ����е㲢�������
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
            //�õ��·�Χ
            IEnvelope2 envelope = e.newEnvelope as IEnvelope2;
            //����һ������Ҫ��
            IRectangleElement rectangleelement = new RectangleElementClass();
            //�õ�һ��Ҫ�ز�������Ҫ�ش���
            IElement element = rectangleelement as IElement;
            //���·�Χ��ֵ��Ҫ�صļ�������
            element.Geometry = envelope;
            //�õ�RGBColor����
            IRgbColor color = new RgbColorClass();
            //����Color��RGBֵ
            color.RGB = 00000255;
            //����͸����
            color.Transparency = 255;
            //����һ����״����
            ILineSymbol lineSymbool = new SimpleLineSymbolClass();
            //������״���ŵ�����
            lineSymbool.Width = 1;
            lineSymbool.Color = color;
            //������ɫ����
            color.RGB = 255;
            color.Transparency = 0;
            //����һ����������������������
            IFillSymbol fillSymbol = new SimpleFillSymbolClass();
            //���������ŵ�����
            //���ɫ
            fillSymbol.Color = color;
            //������
            fillSymbol.Outline = lineSymbool;
            //�õ�һ�����ͼ��Ҫ�أ�AE�����ж�IFillShapElement�Ľ���Ϊ
            /*IFillShapeElement is a generic interface implemented by all 2d elements (CircleElement,
             * EllipseElement, PolygonElement, and RectangleElement).
             * Use this interface when you want to retrieve or set the fill symbol being used by one 
             * of the fill shape elements.
            */
            IFillShapeElement fillShapElement = element as IFillShapeElement;
            //�������Ŵ���Symbol��
            fillShapElement.Symbol = fillSymbol;
            //������ͼ��Ҫ�ظ�ֵ��Ҫ����
            element = fillShapElement as IElement;

            //�õ�ͼ������,����mapcontrol2��map����
            IGraphicsContainer graphicsContainer = axMapControl2.Map as IGraphicsContainer;
            //�õ�һ�����ͼ����graphicscontainer����
            IActiveView activeView = graphicsContainer as IActiveView;

            //�ڻ����µľ��ο�ǰ�����Map�����е��κ�ͼ��Ԫ��
            graphicsContainer.DeleteAllElements();

            //��Ҫ����ӵ�ͼ��������,���������ڶ���0
            graphicsContainer.AddElement(element, 0);
            //��ͼ������װ����ͼ�в�ˢ��
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        private void menuSearch_Click(object sender, EventArgs e)
        {
            Search frmSearch = new Search();
            if (frmSearch.ShowDialog() == DialogResult.OK)
            {
                this.Refresh();
                //��ѯ
                try
                {
                    ILayer layer=GetLayer("zongdi");
                    bool bFind = false;
                    SearchFeature(layer, frmSearch.SelectItem, frmSearch.SelectType, out bFind);
                    if (!bFind)
                    {
                        MessageBox.Show("δ���ҵ������������ڵأ�");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("��ѯ����" + ex.Message);
                }
            }
        }

        public void SearchFeature(ILayer layer, string KeyWords, string FieldName, out bool bFind)
        {
            bFind = false;
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            //��ȡfeatureLayer��featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = string.Format("{0}='{1}'", FieldName, KeyWords);
            featureCusor = featureClass.Search(queryFilter, true);
            //���ҳ����е㲢�������
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
                //��ѯ
                try
                {
                    ILayer layer = GetLayer("zongdi");
                    bool bFind = false;
                    PrintZongdiFeature(layer, frmSearch.SelectItem, out bFind);
                    if (!bFind)
                    {
                        MessageBox.Show("δ���ҵ������������ڵأ�");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("��ѯ����" + ex.Message);
                }
            }
        }

        public void PrintZongdiFeature(ILayer layer, string KeyWords, out bool bFind)
        {
            bFind = false;
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            //��ȡfeatureLayer��featureClass 
            IFeatureClass featureClass = featureLayer.FeatureClass;
            IFeature feature = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            IFeatureCursor featureCusor;
            queryFilter.WhereClause = string.Format("�ڵر��='{0}'",  KeyWords);
            featureCusor = featureClass.Search(queryFilter, true);
            //���ҳ����е㲢�������
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
                frmPrintZongDi.�ڵر�� = KeyWords;
                frmPrintZongDi.Text = "�ڵر�ţ�" + KeyWords;
                int findex = feature.Fields.FindField("Ȩ����");
                frmPrintZongDi.ʹ���� = feature.get_Value(findex).ToString();
                findex = feature.Fields.FindField("ͼ��");
                frmPrintZongDi.ͼ�� = feature.get_Value(findex).ToString();
                findex = feature.Fields.FindField("����");
                frmPrintZongDi.���� = feature.get_Value(findex).ToString();
                findex = feature.Fields.FindField("ʹ��Ȩ���");
                frmPrintZongDi.ʹ����� = feature.get_Value(findex).ToString();
                frmPrintZongDi.ShowDialog();
            }
        }
    }
}