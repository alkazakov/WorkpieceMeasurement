using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkpieceMeasurement
{
    public class WorkpieceOptions : IDisposable
    {
        #region Constructor
        private static WorkpieceOptions _instance;
        private static readonly object SyncRoot = new Object();
        public static WorkpieceOptions GetInstance()
        {
            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null) //-V3054
                        _instance = new WorkpieceOptions();
                }

            }
            return _instance;
        }
        private WorkpieceOptions()
        {
            NcPrograms = new List<string>();
            SelectedNcProgram = null;
            IsZeroOffset = false;
            WorkpieceMeasurement = new double[] { 0, 0, 0 };
            ZeroOffset = new double[] { 0, 0, 0 };

        }
        #endregion
        #region Destructor
        ~WorkpieceOptions()
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
                IsZeroOffset = false;
                WorkpieceMeasurement = null;
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

        public List<string> NcPrograms { get; set; }
        public string SelectedNcProgram { get; set; }
        public bool IsZeroOffset { get; set; }
        public double[] WorkpieceMeasurement { get; set; }
        public double[] ZeroOffset { get; set; }




    }
}
