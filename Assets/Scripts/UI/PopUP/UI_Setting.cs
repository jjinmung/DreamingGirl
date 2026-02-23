using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_Setting : UI_Popup
{
    List<Resolution> resolutions = new List<Resolution>();
    [SerializeField]
    List<RenderPipelineAsset> RenderPipelineAssets;
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

    enum Dropdowns
    {
        QualityDropdown,
        ResolutionDropdown,
        ScreenModeDropdown
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
        Bind<TMP_Dropdown>(typeof(Dropdowns));
        GetImage((int)Images.CloseBtn).gameObject.BindEvent((data) =>
        {
            ClosePopupUI();
        });
        GetImage((int)Images.CloseBtn).gameObject.BindEvent(OnEnter,Define.UIEvent.Enter);
        GetImage((int)Images.CloseBtn).gameObject.BindEvent(OnExit,Define.UIEvent.Exit);
        
        //초기화
        Get<Slider>((int)Sliders.BgmSlider).value = Managers.Sound.BGMVolume;
        Get<Slider>((int)Sliders.SfxSlider).value = Managers.Sound.EffectVolume;
        GetText((int)Texts.BgmValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.BgmSlider).value*100).ToString();
        GetText((int)Texts.SfxValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.SfxSlider).value*100).ToString();
        
        //그래픽 초기화 초기화
        GraphicInit(); 
    }
    
    private void Update()
    {
        Managers.Sound.BGMVolume = Get<Slider>((int)Sliders.BgmSlider).value;
        Managers.Sound.EffectVolume = Get<Slider>((int)Sliders.SfxSlider).value;
        
        GetText((int)Texts.BgmValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.BgmSlider).value*100).ToString();
        GetText((int)Texts.SfxValue).text = Mathf.RoundToInt(Get<Slider>((int)Sliders.SfxSlider).value*100).ToString();
    }

    void GraphicInit()
    {
        resolutions.Clear();
        foreach (var item in Screen.resolutions)
        {
            // 1. 주사율 조건 (60Hz)
            if (Mathf.RoundToInt((float)item.refreshRateRatio.value) != 60)
                continue;

            // 2. 중복 검사 (이미 동일한 가로, 세로 값이 리스트에 있는지 확인)
            bool isDuplicate = resolutions.Any(r => r.width == item.width && r.height == item.height);

            if (!isDuplicate)
            {
                resolutions.Add(item);
            }
        }
        
        var resolution = Get<TMP_Dropdown>((int)Dropdowns.ResolutionDropdown);
        resolution.options.Clear();

        int optionIndex = 0;
        foreach (var item in resolutions)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = item.width + "x" + item.height;
            resolution.options.Add(option);

            if (item.width == Screen.width && item.height == Screen.height)
            {
                Managers.UI.ResolutionIndex = optionIndex;
                resolution.value = optionIndex;
            }
            optionIndex++;
        }
        resolution.RefreshShownValue();
        BindDropDown();
        
        Managers.UI.ScreenMode =Screen.fullScreen;
        Get<TMP_Dropdown>((int)Dropdowns.ScreenModeDropdown).value = Managers.UI.ScreenMode ? 0 : 1;
    }

    void BindDropDown()
    {
        var quality = Get<TMP_Dropdown>((int)Dropdowns.QualityDropdown);
        quality.onValueChanged.AddListener(delegate {
            OnQualityValueChanged(quality);
        });
        
        var resolution = Get<TMP_Dropdown>((int)Dropdowns.ResolutionDropdown);
        resolution.onValueChanged.AddListener(delegate {
            OnResolutionValueChanged(resolution);
        });
        
        var screen = Get<TMP_Dropdown>((int)Dropdowns.ScreenModeDropdown);
        screen.onValueChanged.AddListener(delegate {
            OnScreenModeValueChanged(screen);
        });
    }
    void OnQualityValueChanged(TMP_Dropdown change)
    {
        QualitySettings.SetQualityLevel(change.value);
        QualitySettings.renderPipeline = RenderPipelineAssets[change.value];
    }
    void OnResolutionValueChanged(TMP_Dropdown change)
    {
        Managers.UI.ResolutionIndex = change.value;
        
        Screen.SetResolution(
            resolutions[Managers.UI.ResolutionIndex].width, 
            resolutions[Managers.UI.ResolutionIndex].height, 
            Managers.UI.ScreenMode);
    }
    void OnScreenModeValueChanged(TMP_Dropdown change)
    {
        Managers.UI.ScreenMode = change.value == 0;
        
        Screen.SetResolution(
            resolutions[Managers.UI.ResolutionIndex].width, 
            resolutions[Managers.UI.ResolutionIndex].height, 
            Managers.UI.ScreenMode);
    }
    
}
