﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeenPhone
{
    static class Settings
    {

        public static event EventHandler SaveSettings;


        public static void InvokeSaveSettings(object sender)
        {
            if (SaveSettings != null)
                SaveSettings(sender, null);
        }

        public static Properties.Settings Container { get { return Properties.Settings.Default; } }


    }
}
