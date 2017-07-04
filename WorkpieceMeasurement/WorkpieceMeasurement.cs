//#define DEBUG_TRACE
using NXOpen;
using NXOpen.BlockStyler;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using NXOpen.CAM;
using NXOpen.UF;
using NXOpen.UIStyler;
using NXOpen.Utilities;
using Path = System.IO.Path;

namespace WorkpieceMeasurement
{
    public class WorkpieceMeasurement
    {
        //class members
        private static Session _theSession = null;
        private static NXOpen.UF.UFSession _theUfSession = null;
        private static UI _theUi = null;
        private NXOpen.BlockStyler.BlockDialog _theDialog;
        private NXOpen.BlockStyler.Group _group01;// Block type: Group
        private NXOpen.BlockStyler.ListBox _ncProgram;// Block type: List Box
        private NXOpen.BlockStyler.Group _group02;// Block type: Group
        private NXOpen.BlockStyler.ListBox _operation;// Block type: List Box
        private NXOpen.BlockStyler.Group _group03;// Block type: Group
        private NXOpen.BlockStyler.SelectObject _workpiece;// Block type: Selection
        private WorkpieceObject _workpieceOptions;

        //------------------------------------------------------------------------------
        //Constructor for NX Styler class
        //------------------------------------------------------------------------------
        public WorkpieceMeasurement()
        {
            try
            {
                // Initialize WorkpieceObject
                _workpieceOptions = WorkpieceObject.GetInstance();

                _theSession = Session.GetSession();
                _theUfSession = NXOpen.UF.UFSession.GetUFSession();
                _theUi = UI.GetUI();

                // Load UI "WorkpieceMeasurement.dlx" from assembly Directory
                string theDlxFileName = (AppDomain.CurrentDomain.BaseDirectory+ "WorkpieceMeasurement.dlx");
               _theDialog = _theUi.CreateDialog(theDlxFileName);
                
                // Create Events for UI
                _theDialog.AddOkHandler(new NXOpen.BlockStyler.BlockDialog.Ok(ok_cb));
                _theDialog.AddUpdateHandler(new NXOpen.BlockStyler.BlockDialog.Update(update_cb));
                _theDialog.AddInitializeHandler(new NXOpen.BlockStyler.BlockDialog.Initialize(initialize_cb));
                _theDialog.AddDialogShownHandler(new NXOpen.BlockStyler.BlockDialog.DialogShown(dialogShown_cb));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get NC_Program Groups as List of stings
        /// </summary>
        /// <returns></returns>
        public List<string> GetNcProgramsList()
        {
            List<string> ncProgramslist =new List<string>();

            var workPart = _theSession.Parts.Work;
            var setup = workPart.CAMSetup;
            var camGroupCollection = setup.CAMGroupCollection;

            foreach (var camGroup in camGroupCollection)
            {
                if (camGroup.GetType() == typeof(NCGroup))
                {
                    NCGroup ncProgram = (NCGroup)camGroup;
                    // Delete from the list of NC_Program Groups "SPINDLE" and "POCKET01" Group's Name's
                    // Funnily enough also "SPINDLE" and "POCKET01" are Program Group's...:-)
                    if (ncProgram.Name != "SPINDLE" && ncProgram.Name != "POCKET01")
                    {
                        ncProgramslist.Add(ncProgram.Name);
                    } 
                }
            }
            // Create Dummy list, if NC_Program Groups aren't available
            if (ncProgramslist.Count<1)
            {
               ncProgramslist.Add("NC-Programs aren't available");
            }

            _workpieceOptions.NcPrograms = ncProgramslist;

            return ncProgramslist;
        }

        public List<string> GetCamOperationList(string ncGroupName)
        {
            List<string> camOperationList = new List<string>();
            var workPart = _theSession.Parts.Work;
            var setup = workPart.CAMSetup;
            CAMSetup.View ncGroupView = CAMSetup.View.ProgramOrder;
            var camOperationCollection = setup.CAMOperationCollection.ToArray();
            foreach (var camOperation in camOperationCollection)
            {
                var camOperationParent =camOperation.GetParent(ncGroupView);

                if (camOperationParent.Name == ncGroupName)
                {
                    camOperationList.Add(camOperation.Name);
                } 
            }
            return camOperationList;
        }

        public void InitializeNcProgramListBox()
        {
            _ncProgram.SetListItems(GetNcProgramsList().ToArray());
        }

        public static void Main()
        {
            WorkpieceMeasurement theWorkpieceMeasurement = null;
            try
            {
                theWorkpieceMeasurement = new WorkpieceMeasurement();
                theWorkpieceMeasurement.Show();
                
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            finally
            {
                theWorkpieceMeasurement?.Dispose();
                theWorkpieceMeasurement = null;
            }
        }
        public static int GetUnloadOption(string arg)
        {
            //return System.Convert.ToInt32(Session.LibraryUnloadOption.Explicitly);
            return System.Convert.ToInt32(Session.LibraryUnloadOption.Immediately);
            // return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }

        public static void UnloadLibrary(string arg)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
        }

        //------------------------------------------------------------------------------
        //This method shows the dialog on the screen
        //------------------------------------------------------------------------------
        public NXOpen.UIStyler.DialogResponse Show()
        {
            try
            {
                _theDialog.Show();
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            return 0;
        }

        //------------------------------------------------------------------------------
        //Method Name: Dispose
        //------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_theDialog != null)
            {
                _theDialog.Dispose();
                _theDialog = null;
            }
        }

        //------------------------------------------------------------------------------
        //Callback Name: initialize_cb
        //------------------------------------------------------------------------------
        public void initialize_cb()
        {
            try
            {
                _group01 = (NXOpen.BlockStyler.Group)_theDialog.TopBlock.FindBlock("_group01");
                _group02 = (NXOpen.BlockStyler.Group)_theDialog.TopBlock.FindBlock("_group02");
                _group03 = (NXOpen.BlockStyler.Group)_theDialog.TopBlock.FindBlock("_group03");
                _ncProgram = (NXOpen.BlockStyler.ListBox)_theDialog.TopBlock.FindBlock("_ncProgram");
                _ncProgram.SingleSelect = true;
                _operation = (NXOpen.BlockStyler.ListBox)_theDialog.TopBlock.FindBlock("_Operation");
                _operation.SingleSelect = true;
                _workpiece = (NXOpen.BlockStyler.SelectObject)_theDialog.TopBlock.FindBlock("_workpiece");
                InitializeNcProgramListBox();
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
        }

        //------------------------------------------------------------------------------
        //Callback Name: dialogShown_cb
        //------------------------------------------------------------------------------
        public void dialogShown_cb()
        {
            try
            {
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
        }

        //------------------------------------------------------------------------------
        //Callback Name: update_cb
        //------------------------------------------------------------------------------
        public int update_cb(NXOpen.BlockStyler.UIBlock block)
        {
            try
            {
                if (block == _ncProgram)
                {
                    // Get a selected Value from a list of NC_Program Groups
                    _workpieceOptions.SelectedNcProgram= _ncProgram.SelectedItemString;
                    // Update values in CAM-Operation ListBox
                    _operation.SetListItems(GetCamOperationList(_workpieceOptions.SelectedNcProgram).ToArray());
                }
                else if (block == _operation)
                {
                    // Get a selected Value from a list of CAM Operation
                    _workpieceOptions.SelectedCamOperation = _operation.SelectedItemString;
                }
                else if (block == _workpiece)
                {
                    //Calculate workpiece measurement values
                    GetBoundingBoxValues();
                }
                
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            return 0;
        }

        public int ok_cb()
        {
            int errorCode = 0;
            try
            {
                CreateUserUdeProgramHead();
                CreateCamOperationAttribute(_workpieceOptions.SelectedCamOperation);

                // From GTAC Solution Center "Sample CAM C# program : GetToolPathInformation"
                // Use this Method to get Operation's Coordinates in NX Info Windows
                PrintOperationCoordinates(_workpieceOptions.SelectedCamOperation);
            }
            catch (Exception ex)
            {
                errorCode = 1;
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            return errorCode;
        }
        public PropertyList GetBlockProperties(string blockId)
        {
            PropertyList plist = null;
            try
            {
                plist = _theDialog.GetBlockProperties(blockId);
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            return plist;
        }

        public void PrintOperationCoordinates(string operationName)
        {
            List<int> xList = new List<int>();
            
            var workPart = _theSession.Parts.Work;
            var camOperationCollection = workPart.CAMSetup.CAMOperationCollection;
            var camOperation = camOperationCollection.FindObject(operationName);
            var path = camOperation.GetPath();
            for (int j = 1; j < path.NumberOfToolpathEvents + 1; j++)    /* The Path Index must be Start at 1 */
            {
                CamPathToolpathEventType camPathToolpathEventType = path.GetToolpathEventType(j);
                CamPathMotionType camPathMotionType = default(CamPathMotionType);
                CamPathMotionShapeType camPathMotionShapeType = default(CamPathMotionShapeType);

                switch (camPathToolpathEventType)
                {
                    case CamPathToolpathEventType.Motion:
                        path.IsToolpathEventAMotion(j, out camPathMotionType, out camPathMotionShapeType);
                        Guide.InfoWriteLine(j.ToString() + ". Path Motion Type : " + camPathMotionType + " --> Shape Type : " + camPathMotionShapeType);

                        switch (camPathMotionType)
                        {
                            case CamPathMotionType.From:
                            case CamPathMotionType.Rapid:
                            case CamPathMotionType.Approach:
                            case CamPathMotionType.Engage:
                            case CamPathMotionType.FirstCut:
                            case CamPathMotionType.Cut:
                            case CamPathMotionType.SideCut:
                            case CamPathMotionType.Stepover:
                            case CamPathMotionType.InternalLift:
                            case CamPathMotionType.Retract:
                            case CamPathMotionType.Traversal:
                            case CamPathMotionType.Gohome:
                            case CamPathMotionType.Return:
                            case CamPathMotionType.Departure:
                            case CamPathMotionType.Cycle:
                            case CamPathMotionType.Undefined:

                                switch (camPathMotionShapeType)
                                {
                                    case CamPathMotionShapeType.Linear:
                                        PathLinearMotion pathLinearMotion = path.GetLinearMotion(j);
                                        DisplayMotionInformation(pathLinearMotion);
                                        
                                        break;

                                    case CamPathMotionShapeType.Circular:
                                        PathCircularMotion pathCircularMotion = path.GetCircularMotion(j);
                                        DisplayCircularMotionInformation(pathCircularMotion);
                                        break;

                                    case CamPathMotionShapeType.Helical:
                                        PathHelixMotion pathHelixMotion = path.GetHelixMotion(j);
                                        DisplayHelicalMotionInformation(pathHelixMotion);
                                        break;

                                    case CamPathMotionShapeType.Nurbs:
                                        Guide.InfoWriteLine("Nurbs Motion Shape.");
                                        break;

                                    case CamPathMotionShapeType.Undefined:
                                        Guide.InfoWriteLine("Motion Shape Undefined.");
                                        break;

                                    default:
                                        Guide.InfoWriteLine("Unknown Motion Shape.");
                                        break;
                                } /* switch camPathMotionShapeType */
                                break;

                            default:
                                break;
                        } /* switch camPathMotionType */
                        break;

                    case CamPathToolpathEventType.LevelMarker:
                        PathLevelMarker pathLevelMarker = path.GetLevelMarker(j);
                        double levelDepth = pathLevelMarker.LevelDepth;
                        Vector3d vector3d = pathLevelMarker.LevelNormal;
                        Guide.InfoWriteLine(j.ToString() + ".Level Marker Depth : " + levelDepth + " Normal X" + vector3d.X + " Y" + vector3d.Y + " Z" + vector3d.Z + "\n\n");
                        break;

                    case CamPathToolpathEventType.System:
                        Guide.InfoWriteLine(j.ToString() + ". System Tool Path Event Type\n\n");
                        break;

                    case CamPathToolpathEventType.Ude:
                        Ude ude = path.GetUde(j);
                        DisplayUdeInformation(ude, j);
                        break;

                    default:
                        Guide.InfoWriteLine("Unknown ToolPath Event Type.");
                        break;
                } /* switch camPathToolpathEventType */
            } /* for int j = 1 */


        }

        /// <summary>
        /// Get Measurement Values (X,Y,Z Distances and Zero Point displacement)
        /// from selected Body
        /// Set Values to WorkpieceObject Object
        /// </summary>
        public void GetBoundingBoxValues()
        {
            //Get selected objects from dialog
            TaggedObject[] workpieceObj = _workpiece.GetSelectedObjects();

            //Convert selected objects to body's
            Body bodyWorkpiece= (workpieceObj.Length > 0)? (Body)workpieceObj?[0] : null;

            //output's coordinates variable 
            double[] minCornerWorkpiece = new double[]{ 0, 0, 0 };
            double[,] directionsWorkpiece = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            double[] distancesWorkpiece = new double[] { 0, 0, 0 };

            //Get values from boundingBox
            if (bodyWorkpiece != null)
            {
                _theUfSession.Modl.AskBoundingBoxExact(bodyWorkpiece.Tag, 0, minCornerWorkpiece, directionsWorkpiece,
                    distancesWorkpiece);
            }

            //Set Values to WorkpieceOption object
            _workpieceOptions.WorkpieceMeasurement= distancesWorkpiece;
            _workpieceOptions.ZeroOffset = minCornerWorkpiece;
        }

        public void PrintBoundingBoxValues(double[] boundingBoxValues)
        {
            string printString = "V.P.X=" + boundingBoxValues[0] + ", " +
                                 "V.P.Y=" + boundingBoxValues[1] + ", " +
                                 "V.P.Z=" + boundingBoxValues[2]; 
            Guide.InfoWriteLine(printString);
        }
        /// <summary>
        /// Create UDE "Program Head" and fill UDE's Data from WorkpieceObject Object
        /// </summary>
        public void CreateProgramUde()
        {
            var workPart = _theSession.Parts.Work;
            var camObject = new CAMObject[1];
            var ncGroup = workPart.CAMSetup.CAMGroupCollection.FindObject(_workpieceOptions.SelectedNcProgram);
            camObject[0] = ncGroup;

            var udeSet = workPart.CAMSetup.CreateObjectsUdeSet(camObject, CAMSetup.Ude.Start);
            var ude2 =  udeSet.UdeSet.UdeList;

            var dbk = workPart.CAMSetup.CAMOperationCollection.ToArray();

            foreach (var vOperation in dbk)
            {
                if (vOperation.GetType().ToString() == "NXOpen.CAM.PlanarMilling")
                {
                    var bu = workPart.CAMSetup.CAMOperationCollection.CreatePlanarMillingBuilder(vOperation);
                    bu.Commit();
                }
            }
            // Check if "Program Head" is already available. if this true -> delete this UDE and create new one
            int i = 0;
            foreach (var vUde in ude2.GetContents())
            { 
                if (vUde.UdeName == "PROGRAMMKOPF"){
                    ude2.Erase(i);
                }
                i++;
            }
            var ude = (Ude)udeSet.UdeSet.CreateUdeByName("PROGRAMMKOPF");

            // WorkpieceObject measurement data - > to UDE "PROGRAMKOPF"
            var vpx = ude.GetParameter("vpx").DoubleValue =_workpieceOptions.WorkpieceMeasurement[0];
            var vpy = ude.GetParameter("vpy").DoubleValue = _workpieceOptions.WorkpieceMeasurement[1];
            var vpz = ude.GetParameter("vpz").DoubleValue = _workpieceOptions.WorkpieceMeasurement[2];

            //Zero point displacement - > to UDE "PROGRAMKOPF"
            var p41n = ude.GetParameter("p41n").DoubleValue = _workpieceOptions.ZeroOffset[0];
            var p42n = ude.GetParameter("p42n").DoubleValue = _workpieceOptions.ZeroOffset[1];
            var p43n = ude.GetParameter("p43n").DoubleValue = _workpieceOptions.ZeroOffset[2];
            
            udeSet.UdeSet.UdeList.Append(ude);
            udeSet.Commit();
        }

        /// <summary>
        /// Create automatic UDE "Program Head" with WorkpieceObject Measurement Values
        /// </summary>
        public void CreateUserUdeProgramHead()
        {
            var workPart = _theSession.Parts.Work;
            var camObject = new CAMObject[1];
            var ncGroup = workPart.CAMSetup.CAMGroupCollection.FindObject(_workpieceOptions.SelectedNcProgram);
            camObject[0] = ncGroup;
            
            var udeSet = workPart.CAMSetup.CreateObjectsUdeSet(camObject, CAMSetup.Ude.Start);
            var userUdeList = udeSet.UdeSet.UdeList;

            // Check if UDE already available. If this true -> Delete this one
            for (int j = 0; j < userUdeList.GetContents().Length; j++)
            {
                if (userUdeList.GetContents()[j].UdeName == "PROGRAMMKOPF")
                {
                    userUdeList.ClearIndex(j);
                }
            }

            //Create UDE "Program Head" and fill UDE with WorkpieceObject Measurement Values
            var userUde = (Ude)udeSet.UdeSet.CreateUdeByName("PROGRAMMKOPF");

            // WorkpieceObject measurement data - > to UDE "PROGRAMKOPF"
            userUde.GetParameter("vpx").DoubleValue = _workpieceOptions.WorkpieceMeasurement[0];
            userUde.GetParameter("vpy").DoubleValue = _workpieceOptions.WorkpieceMeasurement[1];
            userUde.GetParameter("vpz").DoubleValue = _workpieceOptions.WorkpieceMeasurement[2];

            //Zero point displacement - > to UDE "PROGRAMKOPF"
            userUde.GetParameter("p41n").DoubleValue = _workpieceOptions.ZeroOffset[0];
            userUde.GetParameter("p42n").DoubleValue = _workpieceOptions.ZeroOffset[1];
            userUde.GetParameter("p43n").DoubleValue = _workpieceOptions.ZeroOffset[2];

            udeSet.UdeSet.UdeList.Append(userUde);
            udeSet.Commit();

        }

        /// <summary>
        /// Create CAM Operation attributes from WorkpieceObject Measurement
        /// </summary>
        /// <param name="selectedCamOperation"></param>
        public void CreateCamOperationAttribute(string selectedCamOperation)
        {
            var workPart = _theSession.Parts.Work;

            // Get selected CAM Operation Object
            var camOperationObject =workPart.CAMSetup.CAMOperationCollection.FindObject(selectedCamOperation);
            var dd = camOperationObject.GetParent(CAMSetup.View.Geometry);
            Guide.InfoWriteLine(dd.Name);

           var camObject = NXOpen.Utilities.NXObjectManager.Get(dd.Tag);
           var rt = (FeatureGeometry) camObject;
           Guide.InfoWriteLine(rt.GetParent().GetParent().Name);
            
  
            // Fill CAM Operation Object(attributes) with WorkpieceObject Data
            camOperationObject.SetUserAttribute("Measurement", 0, _workpieceOptions.WorkpieceMeasurement[1], Update.Option.Now);
            camOperationObject.SetUserAttribute("Measurement", 1, _workpieceOptions.WorkpieceMeasurement[1], Update.Option.Now);
            camOperationObject.SetUserAttribute("Measurement", 2, _workpieceOptions.WorkpieceMeasurement[2], Update.Option.Now);

            camOperationObject.SetUserAttribute("ZeroPoint", 0, _workpieceOptions.ZeroOffset[0], Update.Option.Now);
            camOperationObject.SetUserAttribute("ZeroPoint", 1, _workpieceOptions.ZeroOffset[1], Update.Option.Now);
            camOperationObject.SetUserAttribute("ZeroPoint", 2, _workpieceOptions.ZeroOffset[2], Update.Option.Now);
        }

        #region Display Information
        // There Method's are from GTAC Solution Center
        // Sample CAM C# program : GetToolPathInformation
        // https://solutions.industrysoftware.automation.siemens.com/view.php?sort=desc&p=1&q=%5CtLinear+Motion+End+Point&file_type=text&i=nx_api5803&k=0&o=0

        static void DisplayUdeInformation(Ude ude, int count)
        {
            string udeName = ude.UdeName;
            int numberOfparameters = ude.NumberOfParameters;

            Guide.InfoWriteLine(count.ToString() + ". Ude Name : " + udeName + " Number of Parameters : " + numberOfparameters);
        }

        //------------------------------------------------------------------------------
        //             Method : DisplayHelicalMotionInformation
        //
        //  Display Helical Motion Information
        //
        //  Input PathHelixMotion : The Helical Path Motion to Query
        //  Return                : None
        //------------------------------------------------------------------------------
        static void DisplayHelicalMotionInformation(PathHelixMotion pathHelixMotion)
        {
            Point3d arcCenter = pathHelixMotion.ArcCenter;
            Point3d endPoint = pathHelixMotion.EndPoint;

            double feedValue = 0.0;
            double arcRadius = pathHelixMotion.ArcRadius;
            double numberOfRevolutions = pathHelixMotion.NumberOfRevolutions;

            CamPathDir camPathDir = pathHelixMotion.Direction;
            CamPathFeedUnitType camPathFeedUnitType;

            Vector3d vector3d = pathHelixMotion.ToolAxis;

            pathHelixMotion.GetFeedrate(out feedValue, out camPathFeedUnitType);

            Guide.InfoWriteLine("\tCircular Motion End Point : X" + endPoint.X + " Y" + endPoint.Y + " Z" + endPoint.Z);
            Guide.InfoWriteLine("\tCenter      : X" + arcCenter.X + " Y" + arcCenter.Y + " Z" + arcCenter.Z);
            Guide.InfoWriteLine("\tRadius      : " + arcRadius + " --> Direction : " + camPathDir + " --> Number of Revolutions : " + numberOfRevolutions);
            Guide.InfoWriteLine("\tTool Axis   : X" + vector3d.X + " Y" + vector3d.Y + " Z" + vector3d.Z);
            Guide.InfoWriteLine("\tMotion Type : " + pathHelixMotion.MotionType);
            Guide.InfoWriteLine("\tFeedRate    : " + feedValue + " --> Unit : " + camPathFeedUnitType + "\n\n");
        }

        //------------------------------------------------------------------------------
        //             Method : DisplayCircularMotionInformation
        //
        //  Display Circular Motion Information
        //
        //  Input PathCircularMotion : The Circular Path Motion to Query
        //  Return                   : None
        //------------------------------------------------------------------------------
        static void DisplayCircularMotionInformation(PathCircularMotion pathCircularMotion)
        {
            double feedValue = 0.0;
            double arcRadius = pathCircularMotion.ArcRadius;

            Point3d arcCenter = pathCircularMotion.ArcCenter;
            Point3d endPoint = pathCircularMotion.EndPoint;

            CamPathDir camPathDir = pathCircularMotion.Direction;
            CamPathFeedUnitType camPathFeedUnitType;

            Vector3d vector3d = pathCircularMotion.ToolAxis;

            pathCircularMotion.GetFeedrate(out feedValue, out camPathFeedUnitType);

            Guide.InfoWriteLine("\tCircular Motion End Point : X" + endPoint.X + " Y" + endPoint.Y + " Z" + endPoint.Z);
            Guide.InfoWriteLine("\tCenter      : X" + arcCenter.X + " Y" + arcCenter.Y + " Z" + arcCenter.Z);
            Guide.InfoWriteLine("\tRadius      : " + arcRadius + " --> Direction : " + camPathDir);
            Guide.InfoWriteLine("\tTool Axis   : X" + vector3d.X + " Y" + vector3d.Y + " Z" + vector3d.Z);
            Guide.InfoWriteLine("\tMotion Type : " + pathCircularMotion.MotionType);
            Guide.InfoWriteLine("\tFeedRate    : " + feedValue + " --> Unit : " + camPathFeedUnitType + "\n\n");
        }

        //------------------------------------------------------------------------------
        //             Method : DisplayMotionInformation
        //
        //  Display Linear Motion Information
        //
        //  Input PathLinearMotion : The Linear Path Motion to Query
        //  Return                 : None
        //------------------------------------------------------------------------------
        static void DisplayMotionInformation(PathLinearMotion pathLinearMotion)
        {
            double feedValue = 0.0;
            CamPathFeedUnitType camPathFeedUnitType;

            Vector3d vector3d = pathLinearMotion.ToolAxis;
            Point3d endPoint = pathLinearMotion.EndPoint;

            pathLinearMotion.GetFeedrate(out feedValue, out camPathFeedUnitType);
            Guide.InfoWriteLine("\tLinear Motion End Point : X" + endPoint.X + " Y" + endPoint.Y + " Z" + endPoint.Z);
            Guide.InfoWriteLine("\tTool Axis   : X" + vector3d.X + " Y" + vector3d.Y + " Z" + vector3d.Z);
            Guide.InfoWriteLine("\tMotion Type : " + pathLinearMotion.MotionType);
            Guide.InfoWriteLine("\tFeedRate    : " + feedValue + " --> Unit : " + camPathFeedUnitType + "\n\n");
        }

        //------------------------------------------------------------------------------
        //             Method : SystemInfo
        //
        //  Display System Information
        //
        //  Input UFPart : The UF Part
        //  Input Part   : The Work Part
        //  Return       : None
        //------------------------------------------------------------------------------
        static void SystemInfo(UFPart uFPart, Part workPart)
        {
            SystemInfo sysInfo;
            _theUfSession.UF.AskSystemInfo(out sysInfo);

            string partName = string.Empty;
            uFPart.AskPartName(workPart.Tag, out partName);

            Guide.InfoWriteLine("============================================================");
            Guide.InfoWriteLine("Information Listing Created by : " + sysInfo.user_name.ToString());
            Guide.InfoWriteLine("Date                           : " + sysInfo.date_buf.ToString());
            Guide.InfoWriteLine("Current Work Part              : " + partName);
            Guide.InfoWriteLine("Node Name                      : " + sysInfo.node_name.ToString());
            Guide.InfoWriteLine("============================================================\n\n");
        }

        #endregion
    }

}
