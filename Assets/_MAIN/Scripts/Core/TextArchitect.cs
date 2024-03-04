using System.Collections;
using UnityEngine;
using TMPro;
using UnityEditor;

/// <summary>
/// Build and reveal any text dynamically
/// </summary>
public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;

    #region Text definitions and variables
    /// <summary>
    /// Will assign which tmro should be used, the UI or World
    /// </summary>
    public TMP_Text _tmPro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    public string currentText => _tmPro.text;
    public string targetText {  get; private set; } = string.Empty;

    /// <summary>
    /// preText is used to build new text in front of the already builded text.
    /// </summary>
    public string preText { get; private set; } = string.Empty;

    private int preTextLenght = 0;

    public string fullTargetText => preText + targetText;

    public enum BuildMethod 
    { 
        instant,
        typewriter,
        fade
    }
    
    public BuildMethod buildMethod = BuildMethod.instant;

    public Color textColor 
    { 
        get { return _tmPro.color;} 
        set { _tmPro.color = value; }
    }
    

    /// <summary>
    /// Universal speed for typwrite or fade effect
    /// </summary>
    private const float baseSpeed = 1;
    /// <summary>
    /// Nultiplier for typewrite and fade effect. This can be changed in configuration
    /// </summary>
    private float speedMultiplier = 1;
    /// <summary>
    /// The base speed times the speedMultiplies. The calculation is already done here.
    /// </summary>
    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    /// <summary>
    /// Used to make the text appears faster.
    /// </summary>
    public bool hurryUp = false;
    private int characterMultiplier = 1;
    public int charactersPerCycle { get { return speed <= 2f ? characterMultiplier : speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3; } }
    
    private const float typeWriterSpeedBaseValue = 0.015f;
    #endregion

    public TextArchitect(TextMeshProUGUI tmpro_ui) => this.tmpro_ui = tmpro_ui;
    public TextArchitect(TextMeshPro tmpro_world) => this.tmpro_world = tmpro_world;

    /// <summary>
    /// Build the string in the tmp pro
    /// </summary>
    public Coroutine Build(string text)
    {
        preText = string.Empty;
        targetText = text;

        Stop();

        buildProcess = _tmPro.StartCoroutine(Building());
        return buildProcess;
    }

    /// <summary>
    /// Same as Build, but appending the text
    /// </summary>
    public Coroutine Append(string text)
    {
        preText = _tmPro.text;
        targetText = text;

        Stop();
        buildProcess = _tmPro.StartCoroutine(Building());
        return buildProcess;
    }

    /// <summary>
    /// holds the coroutine to text builder. This will be used to stop the build when necessary
    /// </summary>
    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;

    public void Stop()
    {
        if (!isBuilding)
            return;

        _tmPro.StopCoroutine(buildProcess);
        OnComplete();
    }

    IEnumerator Building()
    {
        Prepare();

        switch (buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }
        OnComplete();
    }

    private void OnComplete()
    {
        buildProcess = null;
        hurryUp = false;
    }

    public void ForceComplete()
    {
        switch (buildMethod)
        {
            case BuildMethod.typewriter:
            case BuildMethod.fade:
                _tmPro.maxVisibleCharacters = _tmPro.textInfo.characterCount; 
                break;
        }
        Stop();
    }


    /// <summary>
    /// Prepare the tmpro status before starts building or appending
    /// </summary>
    private void Prepare()
    {
        switch (buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
;                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter(); 
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;
        }
    }

    private void Prepare_Instant()
    {
        _tmPro.color = _tmPro.color;
        _tmPro.text = fullTargetText;
        _tmPro.ForceMeshUpdate();
        _tmPro.maxVisibleCharacters = _tmPro.textInfo.characterCount;
    }
        
    private void Prepare_Typewriter()
    {
        _tmPro.color = _tmPro.color;
        _tmPro.maxVisibleCharacters = 0;
        _tmPro.text = preText;

        if(preText != "")
        {
            _tmPro.ForceMeshUpdate();
            _tmPro.maxVisibleCharacters = _tmPro.textInfo.characterCount;
        }

        _tmPro.text += targetText;
        _tmPro.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {
        _tmPro.text = preText;
        if(preText != "")
        {
            _tmPro.ForceMeshUpdate();
            preTextLenght = _tmPro.textInfo.characterCount;
        }
        else
            preTextLenght = 0;
    
        _tmPro.text += targetText;
        _tmPro.maxVisibleCharacters = int.MaxValue;
        _tmPro.ForceMeshUpdate();

        TMP_TextInfo textInfo = _tmPro.textInfo;
        Color colorVisable = new Color(textColor.r, textColor.g, textColor.b, 1);
        Color colorHidden = new Color(textColor.r, textColor.g, textColor.b, 0);

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        for(int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            if(i < preTextLenght)
            {
                for (int vertexIterator = 0; vertexIterator < 4; vertexIterator++)
                    vertexColors[charInfo.vertexIndex + vertexIterator] = colorVisable;
            }
            else
            {
                for (int vertexIterator = 0; vertexIterator < 4; vertexIterator++)
                    vertexColors[charInfo.vertexIndex + vertexIterator] = colorHidden;
            }
        }

        _tmPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private IEnumerator Build_Typewriter()
    {
        while (_tmPro.maxVisibleCharacters < _tmPro.textInfo.characterCount)
        {
            _tmPro.maxVisibleCharacters += SpeedCicle();
            yield return new WaitForSeconds(typeWriterSpeedBaseValue / speed);
        }
    }

    private IEnumerator Build_Fade()
    {
        int minRange = preTextLenght;
        int maxRange = minRange + 1;

        byte alphaThreshold = 15;

        TMP_TextInfo textInfo = _tmPro.textInfo;

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;

        float[] alphaValues = new float[textInfo.characterCount];
        float fadeSpeed = (SpeedCicle() * speed)*3f;

        while (true)
        {

            for (int i = minRange; i < maxRange; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                alphaValues[i] = Mathf.MoveTowards(alphaValues[i], 255, fadeSpeed);

                for (int vertexIterator = 0; vertexIterator < 4; vertexIterator++)
                    vertexColors[charInfo.vertexIndex + vertexIterator].a = (byte)alphaValues[i];

                if (alphaValues[i] >= 255)
                    minRange++;
            }

            _tmPro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            bool lastCharacterIsInvisible = !textInfo.characterInfo[maxRange - 1].isVisible;

            if (alphaValues[maxRange - 1] > alphaThreshold || lastCharacterIsInvisible)
            {
                if (maxRange < textInfo.characterCount)
                    maxRange++;
                else if (alphaValues[maxRange - 1] >= 255 || lastCharacterIsInvisible)
                    break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private int SpeedCicle() => hurryUp ? charactersPerCycle * 5 : charactersPerCycle;
}
