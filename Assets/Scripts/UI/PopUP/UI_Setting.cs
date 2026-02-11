using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    enum Texts
    {
        BgmValue,
        SfxValue
    }

    enum Sliders
    {
        BgmSlider,
        SfxSlider
    }

    enum Images
    {
        CloseBtn,
    }
    

    private void Start()
    {
        Init();
    }
    
    public override void Init()
    {
        base.Init();
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));
        
        GetImage((int)Images.CloseBtn).gameObject.BindEvent((data) =>
        {
            Managers.UI.ClosePopupUI(this);
        });
        GetImage((int)Images.CloseBtn).gameObject.BindEvent(OnEnter,Define.UIEvent.Enter);
        GetImage((int)Images.CloseBtn).gameObject.BindEvent(OnExit,Define.UIEvent.Exit);
        
        //초기화
        Get<Slider>((int)Sliders.BgmSlider).value = Managers.Sound.BGMVolume;
        Get<Slider>((int)Sliders.SfxSlider).value = Managers.Sound.EffectVolume;
        GetText((int)Texts.BgmValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.BgmSlider).value*100).ToString();
        GetText((int)Texts.SfxValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.SfxSlider).value*100).ToString();

    }

    private void Update()
    {
        Managers.Sound.BGMVolume = Get<Slider>((int)Sliders.BgmSlider).value;
        Managers.Sound.EffectVolume = Get<Slider>((int)Sliders.SfxSlider).value;
        
        GetText((int)Texts.BgmValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.BgmSlider).value*100).ToString();
        GetText((int)Texts.SfxValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.SfxSlider).value*100).ToString();
    }
}
