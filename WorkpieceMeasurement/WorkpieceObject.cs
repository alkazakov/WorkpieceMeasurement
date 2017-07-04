using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkpieceMeasurement
{
    /// <summary>
    /// Singleton Pattern for WorkpieceObject Measurement Data
    /// </summary>
    public class WorkpieceObject : IDisposable
    {
        #region Constructor
        private static WorkpieceObject _instance;
        private static readonly object SyncRoot = new Object();
        public static WorkpieceObject GetInstance()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null) //-V3054
                        _instance = new WorkpieceObject();
                }
            }
            return _instance;
        }
        private WorkpieceObject()
        {
            // Initialize Data
            NcPrograms = new List<string>();
            SelectedNcProgram = null;
            SelectedCamOperation = null;
            WorkpieceMeasurement = new double[] { 0, 0, 0 };
            ZeroOffset = new double[] { 0, 0, 0 };
        }
        #endregion
        #region Destructor
        ~WorkpieceObject()
        {
            Dispose(true);
        }
        #endregion
        #region Disposable
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                NcPrograms = null;
                SelectedNcProgram = null;
                WorkpieceMeasurement = null;
                SelectedCamOperation = null;
                ZeroOffset = null;
            }
            ReleaseUnmanagedResources();
        }
        public void Dispose()
        {
            Dispose(true); 
        }
        private void ReleaseUnmanagedResources()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
        #region Data
        public List<string> NcPrograms { get; set; }
        public string SelectedNcProgram { get; set; }
        public string SelectedCamOperation { get; set; }
        public double[] WorkpieceMeasurement { get; set; }
        public double[] ZeroOffset { get; set; }
        #endregion
    }
}
