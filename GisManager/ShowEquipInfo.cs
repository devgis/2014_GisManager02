using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GisManager
{
    public partial class ShowEquipInfo : Form
    {
        String _EquipID;
        public ShowEquipInfo(String EquipID)
        {
            InitializeComponent();
            _EquipID = EquipID;
        }
    }
}