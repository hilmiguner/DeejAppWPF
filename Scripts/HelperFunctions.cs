using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeejAppWPF.Scripts
{
    class HelperFunctions
    {
        public static float Normalize(float value)
        {
            float newValue = (value / 1021);
            return newValue > 1 ? 1 : newValue;
        }
    }
}
