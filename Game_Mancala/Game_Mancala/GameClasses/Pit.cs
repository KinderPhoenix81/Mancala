using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Mancala
{
    public class Pit
    {
        //Values for the location of a pit
        public int rowIndex;
        public int colIndex;

        //List of stone objects
        public List<Stone> Stones = new List<Stone>();
    }
}
