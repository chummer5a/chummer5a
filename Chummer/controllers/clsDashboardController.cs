using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chummer.controllers
{
    public class clsDashboardController
    {
        #region Singleton

        private static clsDashboardController _instance;
        public static clsDashboardController Instance
        {
            get
            {
                if (clsDashboardController._instance == null)
                    clsDashboardController._instance = new clsDashboardController();

                return clsDashboardController._instance;
            }
        }

        /**
         * Singleton instance, no outside instantiation
         */
        private clsDashboardController()
        {
            // TODO edward
            
        }

        #endregion

        #region Initiative



        #endregion
    }
}
