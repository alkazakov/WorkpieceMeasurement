#define NODEBUG

using NXOpen;
using NXOpen.BlockStyler;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
        private NXOpen.BlockStyler.SelectObject _baseplate;// Block type: Selection
        private NXOpen.BlockStyler.SelectObject _workpiece;// Block type: Selection

        //------------------------------------------------------------------------------
        //Constructor for NX Styler class
        //------------------------------------------------------------------------------
        public WorkpieceMeasurement()
        {
            try
            {
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
                _baseplate = (NXOpen.BlockStyler.SelectObject)_theDialog.TopBlock.FindBlock("baseplate");
                _workpiece = (NXOpen.BlockStyler.SelectObject)_theDialog.TopBlock.FindBlock("workpiece");
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
                if (block == _baseplate)
                {
                }
                else if (block == _workpiece)
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
                PrintBoundingBoxValues(GetBoundingBoxValues());
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double[] GetBoundingBoxValues()
        {
            //BoundingBox dimensions {X,Y,Z}
            double[] boundingBoxValues = { 0, 0, 0 };

            //Get selected objects from dialog
            TaggedObject[] basePlateObj = _baseplate.GetSelectedObjects();
            TaggedObject[] workpieceObj = _workpiece.GetSelectedObjects();

            //Convert selected objects to body's
            Body bodyBasePlate = (Body)basePlateObj[0];
            Body bodyWorkpiece = (Body)workpieceObj[0];

            double[] minCornerBasePlate = { 0, 0, 0 };
            double[,] directionsBasePlate = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            double[] distancesBasePlate = { 0, 0, 0 };

            double[] minCornerWorkpiece = { 0, 0, 0 };
            double[,] directionsWorkpiece = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
            double[] distancesWorkpiece = { 0, 0, 0 };

            //Get values from boundingBox
            _theUfSession.Modl.AskBoundingBoxExact(bodyBasePlate.Tag, 0, minCornerBasePlate, directionsBasePlate, distancesBasePlate);
            _theUfSession.Modl.AskBoundingBoxExact(bodyWorkpiece.Tag,0,minCornerWorkpiece,directionsWorkpiece,distancesWorkpiece);

            boundingBoxValues[0] = (distancesBasePlate[0] > distancesWorkpiece[0]) ? distancesBasePlate[0] : distancesWorkpiece[0];
            boundingBoxValues[1] = (distancesBasePlate[1] > distancesWorkpiece[1]) ? distancesBasePlate[1] : distancesWorkpiece[1];
            boundingBoxValues[2] = distancesBasePlate[2] + distancesWorkpiece[2];
#if DEBUG
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

    }

}
