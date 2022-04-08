using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RPCtool
{
    static class Utils {
        public static List<Control> controls = new List<Control>();
        public static List<Control> GetAllControls(Control container)
        {
            foreach (Control c in container.Controls)
            {
                controls.Concat(GetAllControls(c)).ToList();
                controls.Add(c);
            }
            return controls;
        }
    }
}
