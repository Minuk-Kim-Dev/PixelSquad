﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameManager
{
    public bool InGame { get; set; } = false;
    public int Rank { get; set; }
    public int CameraIndex { get; set; } = 0;
}