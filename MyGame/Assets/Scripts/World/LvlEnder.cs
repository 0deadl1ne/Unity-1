﻿using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

public class LvlEnder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ServiceManager.Instanse.EndLevel();
    }
}
