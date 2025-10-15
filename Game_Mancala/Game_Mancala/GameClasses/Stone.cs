using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Mancala
{
    public class Stone
    {
        //Constructor for the stone object
        public Stone(int id, string clr, int pntValue)
        {
            StoneID = id;
            Color = clr;
            PointValue = pntValue;
        }
        //Stores id of a stone
        public int StoneID;

        //Stores the color of a stone
        public string Color;

        //Stores the point value of the stone
        public int PointValue;
    }
}
