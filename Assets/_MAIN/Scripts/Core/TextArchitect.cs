using System.Collections;
using UnityEngine;
using TMPro;

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
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;

    public string currentText => tmpro.text;
    public string targetText {  get; private set; } = string.Empty;

    /// <summary>
    /// preText is used to build new text in front of the already builded text.
    /// </summary>
    public string preText { get; private set; } = string.Empty;

    private int preTextLenght = 0;

    public string fullTargetText => preText + targetText;

    public enum BuildMethod { instant, typewriter, fade} 
    public BuildMethod buildMethod = BuildMethod.instant;

    public Color textColor { get { return tmpro.color;} set { tmpro.color = value; } }
    

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

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    /// <summary>
    /// Same as Build, but appending the text
    /// </summary>
    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;

        Stop();
        buildProcess = tmpro.StartCoroutine(Building());
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

        tmpro.StopCoroutine(buildProcess);
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
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount; 
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
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }
        
    private void Prepare_Typewriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if(preText is not "")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {

    }

    private IEnumerator Build_Typewriter()
    {
        while (tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp? charactersPerCycle * 5 : charactersPerCycle;
            yield return new WaitForSeconds(typeWriterSpeedBaseValue / speed);
        }
    }

    private IEnumerator Build_Fade()
    {
        yield return null;
    }
}
