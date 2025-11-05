using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class MinecraftFogManager : MonoBehaviour 
{
    [Header("Time of Day")]
    [Range(0, 24)] 
    public float timeOfDay = 12f;
    public AnimationCurve timeBrightnessCurve;
    
    [Header("Weather")]
    [Range(0, 1)] 
    public float rainStrength = 0f;
    public float rainTransitionSpeed = 0.5f;
    
    [Header("Sun Settings")]
    public Light sunLight;
    public Gradient sunColorGradient;
    
    [Header("Fog Overrides")]
    public bool overrideUnityFog = true;
    public AnimationCurve fogDensityCurve;
    
    private float currentRainStrength = 0f;
    
    void OnEnable() 
    {
        if (sunLight == null) 
        {
            sunLight = RenderSettings.sun;
        }
        
        // Инициализируем кривые если пустые
        if (timeBrightnessCurve == null || timeBrightnessCurve.keys.Length == 0) 
        {
            timeBrightnessCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }
    
    void Update() 
    {
        UpdateTimeOfDay();
        UpdateWeather();
        UpdateShaderGlobals();
        
        if (overrideUnityFog) 
        {
            UpdateUnityFog();
        }
    }
    
    void UpdateTimeOfDay() 
    {
        // Автоматическая смена времени (опционально)
        if (Application.isPlaying) 
        {
            // timeOfDay += Time.deltaTime * 0.1f; // Скорость смены дня/ночи
            if (timeOfDay > 24) timeOfDay -= 24;
        }
        
        // Позиция солнца как в Minecraft
        if (sunLight != null) 
        {
            float sunAngle = (timeOfDay / 24f) * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
            
            // Цвет солнца
            float t = timeOfDay / 24f;
            if (sunColorGradient != null) 
            {
                sunLight.color = sunColorGradient.Evaluate(t);
            }
            
            // Интенсивность
            float dayIntensity = Mathf.Clamp01((timeOfDay - 6f) / 12f) * 
                                 Mathf.Clamp01((18f - timeOfDay) / 12f);
            sunLight.intensity = dayIntensity;
        }
    }
    
    void UpdateWeather() 
    {
        // Плавный переход погоды
        currentRainStrength = Mathf.Lerp(currentRainStrength, rainStrength, 
                                         Time.deltaTime * rainTransitionSpeed);
    }
    
    void UpdateShaderGlobals() 
    {
        // Brightness: 0 = полночь, 1 = полдень
        float normalizedTime = timeOfDay / 24f;
        float brightness = Mathf.Cos((timeOfDay - 6f) * Mathf.PI / 12f) * 0.5f + 0.5f;
        
        if (timeBrightnessCurve != null && timeBrightnessCurve.keys.Length > 0) 
        {
            brightness = timeBrightnessCurve.Evaluate(brightness);
        }
        
        // Устанавливаем глобальные переменные для всех шейдеров
        Shader.SetGlobalFloat("_GlobalTimeBrightness", brightness);
        Shader.SetGlobalFloat("_GlobalRainStrength", currentRainStrength);
        
        if (sunLight != null) 
        {
            Vector3 sunDir = -sunLight.transform.forward;
            Shader.SetGlobalVector("_GlobalSunDirection", sunDir);
        }
    }
    
    void UpdateUnityFog() 
    {
        // Синхронизируем с Unity Fog для совместимости
        float brightness = Shader.GetGlobalFloat("_GlobalTimeBrightness");
        
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = fogDensityCurve != null ? 
                                    fogDensityCurve.Evaluate(brightness) * 0.01f : 
                                    0.01f;
        
        // Цвет тумана меняется со временем
        Color fogColor = Color.Lerp(
            new Color(0.1f, 0.1f, 0.3f), // Ночь
            new Color(0.7f, 0.8f, 0.9f), // День
            brightness
        );
        
        // Добавляем эффект дождя
        fogColor = Color.Lerp(fogColor, new Color(0.5f, 0.5f, 0.6f), 
                              currentRainStrength);
        
        RenderSettings.fogColor = fogColor;
    }
}