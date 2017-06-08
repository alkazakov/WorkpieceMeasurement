//#define DEBUG_TRACE

using NXOpen;
using NXOpen.BlockStyler;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NXOpen.CAM;
using NXOpen.Utilities;


namespace WorkpieceMeasurement
{
    
    
    public class WorkpieceMeasurement
    {
        //class members
        private static Session _theSession = null;
        private static NXOpen.UF.UFSession _theUfSession = null;
        private static UI _theUi = null;
        private NXOpen.BlockStyler.BlockDialog _theDialog;
        private NXOpen.BlockStyler.Group _group0;// Block type: Group
        private NXOpen.BlockStyler.ListBox _ncProgram;// Block type: List Box
        private NXOpen.BlockStyler.Group _group02;// Block type: Group
        private NXOpen.BlockStyler.Toggle _zeroOffset;// Block type: Toggle
        private NXOpen.BlockStyler.Group _group01;// Block type: Group
        private NXOpen.BlockStyler.SelectObject _workpiece;// Block type: Selection
        private NXOpen.BlockStyler.SelectObject _baseplate;// Block type: Selection
        private WorkpieceOptions _workpieceOptions;

        //------------------------------------------------------------------------------
        //Constructor for NX Styler class
        //------------------------------------------------------------------------------
        public WorkpieceMeasurement()
        {
            try
            {
                _workpieceOptions = WorkpieceOptions.GetInstance();
                _theSession = Session.GetSession();
                _theUfSession = NXOpen.UF.UFSession.GetUFSession();
                _theUi = UI.GetUI();
                string theDlxFileName = "WorkpieceMeasurement.dlx";
                _theDialog = _theUi.CreateDialog(theDlxFileName);
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
                    if (ncProgram.Name != "SPINDLE" && ncProgram.Name != "POCKET01")
                    {
                        ncProgramslist.Add(ncProgram.Name);
                    }
                    
                }
            }
            
            if (ncProgramslist.Count<1)
            {
               ncProgramslist.Add("NC-Programs aren't available");
            }
            _workpieceOptions.NcPrograms = ncProgramslist;
            return ncProgramslist;
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
                _group0 = (NXOpen.BlockStyler.Group)_theDialog.TopBlock.FindBlock("group0");
                _group01 = (NXOpen.BlockStyler.Group)_theDialog.TopBlock.FindBlock("group01");
                _group02 = (NXOpen.BlockStyler.Group)_theDialog.TopBlock.FindBlock("group02");
                _ncProgram = (NXOpen.BlockStyler.ListBox)_theDialog.TopBlock.FindBlock("_ncProgram");
                _ncProgram.SingleSelect = true;
                _zeroOffset = (NXOpen.BlockStyler.Toggle)_theDialog.TopBlock.FindBlock("_zeroOffset");
                _zeroOffset.Value = false;
                _baseplate = (NXOpen.BlockStyler.SelectObject)_theDialog.TopBlock.FindBlock("_baseplate");
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
            //string message=null;
            try
            {
                if (block == _ncProgram)
                {
                    _workpieceOptions.SelectedNcProgram= _ncProgram.SelectedItemString;
                    //Guide.InfoWriteLine(_workpieceOptions.SelectedNcProgram);
                }
                else if (block == _zeroOffset)
                {
                    _baseplate.Enable = !_zeroOffset.Value;
                    _workpieceOptions.IsZeroOffset = _zeroOffset.Value;
                    //message = (_workpieceOptions.IsZeroOffset) ? "on" : "off";
                    //Guide.InfoWriteLine(message);
                }
                else if (block == _workpiece)
                {
                }
                else if (block == _baseplate)
                {
                }
            }
            catch (Exception ex)
            {
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            return 0;
        }

        //------------------------------------------------------------------------------
        //Callback Name: ok_cb
        //------------------------------------------------------------------------------
        public int ok_cb()
        {
            int errorCode = 0;
            try
            {
                //PrintBoundingBoxValues(GetBoundingBoxValues());
                CreateProgramUde();
            }
            catch (Exception ex)
            {
                errorCode = 1;
                _theUi.NXMessageBox.Show("Block Styler", NXMessageBox.DialogType.Error, ex.ToString());
            }
            return errorCode;
        }

        //------------------------------------------------------------------------------
        //Function Name: GetBlockProperties
        //Returns the propertylist of the specified BlockID
        //------------------------------------------------------------------------------
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


        public double[] GetBoundingBoxValues()
        {
            //BoundingBox dimensions {X,Y,Z}
            double[] boundingBoxValues = { 0, 0, 0 };

            //Get selected objects from dialog
            TaggedObject[] basePlateObj = (!_workpieceOptions.IsZeroOffset) ? _baseplate.GetSelectedObjects() : null;
            TaggedObject[] workpieceObj = _workpiece.GetSelectedObjects();

            //Convert selected objects to body's
            Body bodyBasePlate = (Body)basePlateObj?[0];
            Body bodyWorkpiece = (Body)workpieceObj?[0];

            double[] minCornerBasePlate = { 0, 0, 0 };
            double[,] directionsBasePlate = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            double[] distancesBasePlate = { 0, 0, 0 };

            double[] minCornerWorkpiece = { 0, 0, 0 };
            double[,] directionsWorkpiece = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            double[] distancesWorkpiece = { 0, 0, 0 };

            //Get values from boundingBox
            if (bodyBasePlate != null)
            {
                _theUfSession.Modl.AskBoundingBoxExact(bodyBasePlate.Tag, 0, minCornerBasePlate, directionsBasePlate,
                    distancesBasePlate);
            }

            if (bodyWorkpiece != null)
            {
                _theUfSession.Modl.AskBoundingBoxExact(bodyWorkpiece.Tag, 0, minCornerWorkpiece, directionsWorkpiece,
                    distancesWorkpiece);
            }

            boundingBoxValues[0] = (distancesBasePlate[0] > distancesWorkpiece[0]) ? distancesBasePlate[0] : distancesWorkpiece[0];
            boundingBoxValues[1] = (distancesBasePlate[1] > distancesWorkpiece[1]) ? distancesBasePlate[1] : distancesWorkpiece[1];
            boundingBoxValues[2] =(!_workpieceOptions.IsZeroOffset)? (distancesBasePlate[2] + distancesWorkpiece[2]): distancesWorkpiece[2];

            _workpieceOptions.ZeroOffset[0] = (_workpieceOptions.IsZeroOffset) ? minCornerWorkpiece[0] : 0;
            _workpieceOptions.ZeroOffset[1] = (_workpieceOptions.IsZeroOffset) ? minCornerWorkpiece[1] : 0;
            _workpieceOptions.ZeroOffset[2] = (_workpieceOptions.IsZeroOffset) ? minCornerWorkpiece[2] : 0;

            _workpieceOptions.WorkpieceMeasurement[0] = boundingBoxValues[0];
            _workpieceOptions.WorkpieceMeasurement[1] = boundingBoxValues[1];
            _workpieceOptions.WorkpieceMeasurement[2] = boundingBoxValues[2];
#if DEBUG_TRACE
            Guide.InfoWriteLine("---Distances Base Plate---");
            Guide.InfoWriteLine(distancesBasePlate[0].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(distancesBasePlate[1].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(distancesBasePlate[2].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine("---Distances Workpiece---");
            Guide.InfoWriteLine(distancesWorkpiece[0].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(distancesWorkpiece[1].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(distancesWorkpiece[2].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine("---Min_Corner Base Plate---");
            Guide.InfoWriteLine(minCornerBasePlate[0].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(minCornerBasePlate[1].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(minCornerBasePlate[2].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine("---Min_Corner Workpiece---");
            Guide.InfoWriteLine(minCornerWorkpiece[0].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(minCornerWorkpiece[1].ToString(CultureInfo.InvariantCulture));
            Guide.InfoWriteLine(minCornerWorkpiece[2].ToString(CultureInfo.InvariantCulture));
#endif
            return boundingBoxValues;
        }



        public void PrintBoundingBoxValues(double[] boundingBoxValues)
        {
            string printString = "V.P.X=" + boundingBoxValues[0] + ", " +
                                 "V.P.Y=" + boundingBoxValues[1] + ", " +
                                 "V.P.Z=" + boundingBoxValues[2]; 
            Guide.InfoWriteLine(printString);
        }

        public void CreateProgramUde()
        {
            _workpieceOptions.WorkpieceMeasurement = GetBoundingBoxValues();

            var workPart = _theSession.Parts.Work;
            var camObject = new CAMObject[1];
            var ncGroup = (NCGroup) workPart.CAMSetup.CAMGroupCollection.FindObject(_workpieceOptions.SelectedNcProgram);
            //ncGroup.SetUserAttribute();
            
            camObject[0] = ncGroup;

            var udeSet = workPart.CAMSetup.CreateObjectsUdeSet(camObject, CAMSetup.Ude.Start);
            var ude2 =  udeSet.UdeSet.UdeList;

            var dbk = workPart.CAMSetup.CAMOperationCollection.ToArray();

            foreach (var vOperation in dbk)
            {
                //Guide.InfoWriteLine(vOperation.GetType().ToString());
                //Guide.InfoWriteLine(vOperation.Name);
                //Guide.InfoWriteLine(vOperation.GetToolpathTime().ToString());
                if (vOperation.GetType().ToString() == "NXOpen.CAM.PlanarMilling")
                {
                    //Guide.InfoWriteLine("in Case");
                    var bu = workPart.CAMSetup.CAMOperationCollection.CreatePlanarMillingBuilder(vOperation);
                    bu.FeedsBuilder.SpindleRpmBuilder.Value = 123;
                    //Guide.InfoWriteLine(bu.FeedsBuilder.FeedCutBuilder.Value.ToString());
                    bu.Commit();
                }
            }

            

            bool flagUde = false;

            foreach (var vUde in ude2.GetContents())
            {

                //foreach (var VARIABLE in vUde.GetParameterNames())
                //{
                //    Guide.InfoWriteLine(VARIABLE);
                //}
                
                if (vUde.UdeName == "PROGRAMMKOPF") flagUde = true;
            }

            var ude = (!flagUde)?(Ude)udeSet.UdeSet.CreateUdeByName("PROGRAMMKOPF"): (Ude)udeSet.UdeSet.CreateUdeByName("PROGRAMMKOPF");

            // Workpiece measurement data - > to UDE "PROGRAMKOPF"
            var vpx = ude.GetParameter("vpx").DoubleValue =_workpieceOptions.WorkpieceMeasurement[0];
            var vpy = ude.GetParameter("vpy").DoubleValue = _workpieceOptions.WorkpieceMeasurement[1];
            var vpz = ude.GetParameter("vpz").DoubleValue = _workpieceOptions.WorkpieceMeasurement[2];

            //Zero point displacement - > to UDE "PROGRAMKOPF"
            var p41n = (_workpieceOptions.IsZeroOffset)? ude.GetParameter("p41n").DoubleValue = _workpieceOptions.ZeroOffset[0]:0;
            var p42n = (_workpieceOptions.IsZeroOffset)?ude.GetParameter("p42n").DoubleValue = _workpieceOptions.ZeroOffset[1]:0;
            var p43n = (_workpieceOptions.IsZeroOffset)?ude.GetParameter("p43n").DoubleValue = _workpieceOptions.ZeroOffset[2]:0;
            

            udeSet.UdeSet.UdeList.Append(ude);
            udeSet.Commit();

            //PrintBoundingBoxValues(GetBoundingBoxValues());
            //PrintBoundingBoxValues(_workpieceOptions.WorkpieceMeasurement);
        }

        public void GetMom()
        {
            
        }

    }

}
