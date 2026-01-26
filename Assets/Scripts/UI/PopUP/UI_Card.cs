using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Card : UI_Popup
{
    enum Texts
    {
        Card_Text,
    }
    

    enum Images
    {
        Card_Icon,
        EmptyStar1_Image,
        EmptyStar2_Image,
        EmptyStar3_Image,
        EmptyStar4_Image,
        EmptyStar5_Image,
        Star1_Image,
        Star2_Image,
        Star3_Image,
        Star4_Image,
        Star5_Image
    }

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        
		Bind<TextMeshProUGUI>(typeof(Texts));
		Bind<Image>(typeof(Images));
	}

    public void SetUI()
    {
        
    }
   

   

}
