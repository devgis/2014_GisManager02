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
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.Collections.Generic;
using stdole;

namespace GisManager
{
    public sealed partial class MainForm : Form
    {
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;
        #endregion

        #region class constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //get the MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;

            //非管理员不能添加
            if (LoginForm.UserType != 1)
            {
                cmiAddEquip.Visible = false;
            }


            //加载地图
            string sPath=System.IO.Path.Combine(Application.StartupPath,@"Map\Map.mxd");
            axMapControl1.LoadMxFile(sPath);
            axMapControl1.Refresh();

            PublicDim.ReverseMouseWheel();

            //初始化图层
            CreateEquipLayer();
            //加载数据
            LoadEquipData();
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
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }

        #region 私有方法

        private IFeatureLayer EquipLayer;
        /// <summary>
        /// 创建设备图层
        /// </summary>
        /// <param name="DataSetName"></param>
        /// <param name="AliaseName"></param>
        /// <param name="SpatialRef"></param>
        /// <param name="GeometryType"></param>
        /// <param name="PropertyFields"></param>
        /// <returns></returns>
        private void CreateEquipLayer()
        {
            IWorkspaceFactory workspaceFactory = new InMemoryWorkspaceFactoryClass();
            ESRI.ArcGIS.Geodatabase.IWorkspaceName workspaceName = workspaceFactory.Create("", "MyWorkspace", null, 0);
            ESRI.ArcGIS.esriSystem.IName name = (IName)workspaceName;
            ESRI.ArcGIS.Geodatabase.IWorkspace inmemWor = (IWorkspace)name.Open();
            IField oField = new FieldClass();
            IFields oFields = new FieldsClass();
            IFieldsEdit oFieldsEdit = null;
            IFieldEdit oFieldEdit = null;
            IFeatureClass oFeatureClass = null;
            IFeatureLayer oFeatureLayer = null;
            try
            {
                oFieldsEdit = oFields as IFieldsEdit;
                oFieldEdit = oField as IFieldEdit;

                //创建图元属性
                FieldClass feadName = new FieldClass();
                IFieldEdit edit = feadName;
                edit.Name_2 = "Name";
                edit.Type_2 = esriFieldType.esriFieldTypeString;
                oFieldsEdit.AddField(feadName);

                FieldClass feadID = new FieldClass();
                IFieldEdit editID = feadID;
                editID.Name_2 = "ID";
                editID.Type_2 = esriFieldType.esriFieldTypeString;
                oFieldsEdit.AddField(editID);


                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.AvgNumPoints_2 = 5;
                geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
                geometryDefEdit.GridCount_2 = 1;
                geometryDefEdit.HasM_2 = false;
                geometryDefEdit.HasZ_2 = false;
                geometryDefEdit.SpatialReference_2 = axMapControl1.SpatialReference;
                oFieldEdit.Name_2 = "SHAPE";
                oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                oFieldEdit.GeometryDef_2 = geometryDef;
                oFieldEdit.IsNullable_2 = true;
                oFieldEdit.Required_2 = true;
                oFieldsEdit.AddField(oField);
                oFeatureClass = (inmemWor as IFeatureWorkspace).CreateFeatureClass("EquipDS", oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
                (oFeatureClass as IDataset).BrowseName = "EquipDS";
                oFeatureLayer = new FeatureLayerClass();
                oFeatureLayer.Name = "Equip";
                oFeatureLayer.FeatureClass = oFeatureClass;
            }
            catch
            {
            }
            finally
            {
                try
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oField);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFields);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldsEdit);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldEdit);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(name);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workspaceFactory);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(workspaceName);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(inmemWor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oFeatureClass);
                }
                catch { }

                GC.Collect();
            }
            EquipLayer = oFeatureLayer;
            oFeatureLayer.Selectable = true;
            axMapControl1.Map.AddLayer(oFeatureLayer);

            UniqueValueRenderFlyr(oFeatureLayer);
            CreateAnno(oFeatureLayer);
            axMapControl1.Refresh();
        }

        private void LoadEquipData()
        {
            DataTable dt = DBHelper.Instance.GetDataTable("select * from Equip");
            foreach (DataRow dr in dt.Rows)
            {
                string PersionID = dr["EquipID"].ToString();
                string PersionName = dr["EquipName"].ToString();
                double PosX = Convert.ToDouble(dr["PosX"]);
                double PosY = Convert.ToDouble(dr["PosY"]);

                IFeatureLayer pFeatureLyr = EquipLayer as IFeatureLayer;
                IFeatureClass pFeatCls = pFeatureLyr.FeatureClass;
                IDataset pDataset = pFeatCls as IDataset;
                IWorkspace pWS = pDataset.Workspace;
                IWorkspaceEdit pWorkspaceEdit = pWS as IWorkspaceEdit;
                pWorkspaceEdit.StartEditing(false);
                pWorkspaceEdit.StartEditOperation();
                IFeatureBuffer pFeatureBuffer;
                IFeatureCursor pFeatureCuror;
                IFeature pFeature;
                IPoint pPoint = new PointClass();
                pFeatureBuffer = pFeatCls.CreateFeatureBuffer();
                pFeatureCuror = pFeatCls.Insert(true);
                pFeature = pFeatureBuffer as IFeature;
                pPoint.X = PosX;
                pPoint.Y = PosY;

                int field1 = pFeature.Fields.FindField("Name");
                pFeature.set_Value(field1, PersionName);
                field1 = pFeature.Fields.FindField("ID");
                pFeature.set_Value(field1, PersionID);

                IGeometry pPointGeo = pPoint as IGeometry;
                pFeature.Shape = pPointGeo;
                pFeatureCuror.InsertFeature(pFeatureBuffer);
                pWorkspaceEdit.StopEditOperation();
                pWorkspaceEdit.StopEditing(true);
            }
            
            axMapControl1.Refresh();
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
        }

        ///<summary>
        ///设置要素图层唯一值符号化
        ///</summary>
        ///<param name="pFeatureLayer"></param>
        private void UniqueValueRenderFlyr(IFeatureLayer pFeatureLayer)
        {
            //创建SimpleMarkerSymbolClass对象
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            //创建RgbColorClass对象为pSimpleMarkerSymbol设置颜色
            IRgbColor pRgbColor = new RgbColorClass();
            pRgbColor.Red = 255;
            pSimpleMarkerSymbol.Color = pRgbColor as IColor;
            //设置pSimpleMarkerSymbol对象的符号类型，选择钻石
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;
            //设置pSimpleMarkerSymbol对象大小，设置为５
            pSimpleMarkerSymbol.Size = 5;
            //显示外框线
            pSimpleMarkerSymbol.Outline = true;
            //为外框线设置颜色
            IRgbColor pLineRgbColor = new RgbColorClass();
            pLineRgbColor.Green = 255;
            pSimpleMarkerSymbol.OutlineColor = pLineRgbColor as IColor;
            //设置外框线的宽度
            pSimpleMarkerSymbol.OutlineSize = 1;

            IUniqueValueRenderer pUVRender = new UniqueValueRendererClass();
            pUVRender.AddValue("", "", pSimpleMarkerSymbol as ISymbol);
            pUVRender.DefaultLabel = "Name";
            pUVRender.DefaultSymbol = pSimpleMarkerSymbol as ISymbol;

            //设置IGeofeatureLayer的Renderer属性
            (pFeatureLayer as IGeoFeatureLayer).Renderer= pUVRender as IFeatureRenderer;
        }

        public void CreateAnno(IFeatureLayer pFeatureLayer)
         {

               IGeoFeatureLayer pGeoFLayer = pFeatureLayer as IGeoFeatureLayer;
                //得到图层的标注属性集合对象
         IAnnotateLayerPropertiesCollection pAnnoLayerpRropColl = new AnnotateLayerPropertiesCollectionClass();
                pAnnoLayerpRropColl = pGeoFLayer.AnnotationProperties;

                //清空这个集合中的对象
                //pAnnoLayerpRropColl.Clear();

                //新建一个图层标注引擎对象，设置它的属性
                ILabelEngineLayerProperties pLabelEngineLayerProp = new LabelEngineLayerPropertiesClass();
                pLabelEngineLayerProp.Expression = "Name";

                //创建注记文本的文本符号
                ITextSymbol pTextSym = new TextSymbolClass();
                IRgbColor pRgbColor = new RgbColorClass();
                pRgbColor.Red = 255;
                pRgbColor.Green = 255;
                pRgbColor.Blue = 255;
                pTextSym.Color = pRgbColor;
                pTextSym.Size = 20;
                IFont font = new StdFontClass();
                font.Name = "Times New Roman";
                pTextSym.Font = (IFontDisp)font;
                pLabelEngineLayerProp.Symbol = pTextSym;

                //设置注记文本的位置
         IBasicOverposterLayerProperties pBasicOverposeterLayerProp = new BasicOverposterLayerPropertiesClass();
                pBasicOverposeterLayerProp.FeatureType = esriBasicOverposterFeatureType.esriOverposterPoint;
                pBasicOverposeterLayerProp.FeatureWeight = esriBasicOverposterWeight.esriNoWeight;
                pBasicOverposeterLayerProp.LabelWeight = esriBasicOverposterWeight.esriHighWeight;
                pBasicOverposeterLayerProp.BufferRatio = 0;

                //方式一：标注位于点特征顶部
               //pBasicOverposeterLayerProp.PointPlacementOnTop = true;

                //方式二：标注环绕点特征
   pBasicOverposeterLayerProp.PointPlacementMethod = esriOverposterPointPlacementMethod.esriAroundPoint;
                IPointPlacementPriorities pPointPlacement = new PointPlacementPrioritiesClass();
                pPointPlacement.AboveCenter = 0;
                pPointPlacement.AboveLeft = 0;
                pPointPlacement.AboveRight = 0;
                pPointPlacement.BelowCenter = 1;
                pPointPlacement.BelowLeft = 0;
                pPointPlacement.BelowRight = 0;
                pPointPlacement.CenterLeft = 0;
                pPointPlacement.CenterRight = 0;
                pBasicOverposeterLayerProp.PointPlacementPriorities = pPointPlacement;

                //方式三：标准旋转一定角度
//pBasicOverposeterLayerProp.PointPlacementMethod = esriOverposterPointPlacementMethod.esriSpecifiedAngles;
                //double[] angle = new double[2];
                //angle[0] = 45;
                //angle[1] = 90;
                //pBasicOverposeterLayerProp.PointPlacementAngles = angle;
                
                pLabelEngineLayerProp.BasicOverposterLayerProperties = pBasicOverposeterLayerProp;

                IAnnotateLayerProperties pAnnoLayerProp = (IAnnotateLayerProperties)pLabelEngineLayerProp;
                pAnnoLayerpRropColl.Add(pAnnoLayerProp);

                pGeoFLayer.DisplayField = pLabelEngineLayerProp.Expression;
                pGeoFLayer.DisplayAnnotation = true;

        }
        #endregion

        int PosX = 0;
        int PosY = 0;
        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            PosX = e.x;
            PosY = e.y;
            if (e.button == 2)
            {
                contextMenuStrip1.Show(new System.Drawing.Point(e.x+191, e.y+52));
            }
        }

        private void cmiAddEquip_Click(object sender, EventArgs e)
        {
            AddEquip frmAddEquip = new AddEquip();
            if (frmAddEquip.ShowDialog() == DialogResult.OK)
            {

                IPoint point = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(PosX, PosY); ;

                string PersionID = Guid.NewGuid().ToString();
                string PersionName = frmAddEquip.Name;

                string sql = string.Format("insert into equip values('{0}','{1}',{2},{3})", PersionID, PersionName, point.X, point.Y);
                if (DBHelper.Instance.ExcuteSql(sql))
                {
                    IFeatureLayer pFeatureLyr = EquipLayer as IFeatureLayer;
                    IFeatureClass pFeatCls = pFeatureLyr.FeatureClass;
                    IDataset pDataset = pFeatCls as IDataset;
                    IWorkspace pWS = pDataset.Workspace;
                    IWorkspaceEdit pWorkspaceEdit = pWS as IWorkspaceEdit;
                    pWorkspaceEdit.StartEditing(false);
                    pWorkspaceEdit.StartEditOperation();
                    IFeatureBuffer pFeatureBuffer;
                    IFeatureCursor pFeatureCuror;
                    IFeature pFeature;

                    pFeatureBuffer = pFeatCls.CreateFeatureBuffer();
                    pFeatureCuror = pFeatCls.Insert(true);
                    pFeature = pFeatureBuffer as IFeature;

                    int field1 = pFeature.Fields.FindField("Name");
                    pFeature.set_Value(field1, PersionName);
                    field1 = pFeature.Fields.FindField("ID");
                    pFeature.set_Value(field1, PersionID);

                    IGeometry pPointGeo = point as IGeometry;
                    pFeature.Shape = pPointGeo;
                    pFeatureCuror.InsertFeature(pFeatureBuffer);
                    pWorkspaceEdit.StopEditOperation();
                    pWorkspaceEdit.StopEditing(true);

                    axMapControl1.Refresh();
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null); 
                    MessageBox.Show("保存成功！");
                }
                else
                {
                    MessageBox.Show("保存失败！");
                }
            }
        }

        private void cmiSearch_Click(object sender, EventArgs e)
        {
            SearchForm searchForm = new SearchForm();
            searchForm.ShowDialog();
            if (searchForm.DialogResult == DialogResult.OK)
            {
                this.Refresh();
                axMapControl1.Refresh();
                ESRI.ArcGIS.Geodatabase.IQueryFilter queryFilter =new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
                queryFilter.WhereClause = String.Format("Name like '%{0}%'", searchForm.Name);
                IFeatureLayer pFeatureLayer = EquipLayer;
                IFeatureCursor featureCursor = pFeatureLayer.Search(queryFilter,false);
                ESRI.ArcGIS.Geodatabase.IFeature pFeature;
                 while((pFeature= featureCursor.NextFeature())!=null)
                 {
                    axMapControl1.FlashShape(pFeature.Shape);
                    
                 }
                 axMapControl1.Refresh();
            }
            
        }

        private double ConvertPixelsToMapUnits(IActiveView pActiveView, double pixelUnits)
        {
            // Uses the ratio of the size of the map in pixels to map units to do the conversion
            IPoint p1 = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.UpperLeft;
            IPoint p2 = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.UpperRight;
            int x1, x2, y1, y2;
            pActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p1, out x1, out y1);
            pActiveView.ScreenDisplay.DisplayTransformation.FromMapPoint(p2, out x2, out y2);
            double pixelExtent = x2 - x1;
            double realWorldDisplayExtent = pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;
            return pixelUnits * sizeOfOnePixel;
        }

        private void SelectFeature(int x, int y)
        {
            IMap pMap = axMapControl1.Map;
            IActiveView pActiveView = pMap as IActiveView;
            IFeatureLayer pFeatureLayer = pMap.get_Layer(0) as IFeatureLayer;
            IFeatureClass pFeatureClass = pFeatureLayer.FeatureClass;

            //设置点击点的位置
            IPoint point = pActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            ITopologicalOperator pTOpo = point as ITopologicalOperator;
            double length;
            length = ConvertPixelsToMapUnits(pActiveView, 4);
            IGeometry pBuffer = pTOpo.Buffer(length);
            IGeometry pGeomentry = pBuffer.Envelope;
            //空间滤过器
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = pGeomentry;
            //根据被选择要素的不同，设置不同的空间滤过关系
            switch (pFeatureClass.ShapeType)
            {
                case esriGeometryType.esriGeometryPoint:
                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                    break;
                case esriGeometryType.esriGeometryPolyline:
                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                    break;
                case esriGeometryType.esriGeometryPolygon:
                    pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    break;

            }
            IFeatureSelection pFSelection = pFeatureLayer as IFeatureSelection;
            pFSelection.SelectFeatures(pSpatialFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
            ISelectionSet pSelectionset = pFSelection.SelectionSet;
            ICursor pCursor;
            pSelectionset.Search(null, true, out pCursor);
            IFeatureCursor pFeatCursor = pCursor as IFeatureCursor;
            IFeature pFeature = pFeatCursor.NextFeature();
            if (pFeature != null)
            {
                pMap.SelectFeature(pFeatureLayer, pFeature);
                //pFeature = pFeatCursor.NextFeature();
                int fieldNameIndex = pFeature.Fields.FindField("ID");
                string id= pFeature.get_Value(fieldNameIndex).ToString();
                ShowEquipInfo frmShowEquipInfo = new ShowEquipInfo(id);
                frmShowEquipInfo.ShowDialog();
            }
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            SelectFeature(e.x, e.y);
        }
    }
}