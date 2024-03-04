using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//I'll probably do some unit tests later :)

namespace TESTING
{
    public class testingScriptBuilder : MonoBehaviour
    {
        DialogueSystem _dialogueSystem;
        TextArchitect _textArchitect;
        string[] testingLines = new string[5] 
        {
            "Aqui acontece a linha 1 e bora deixar grande pra testar o hurry",
            "Temos também a linha 2 e bora deixar grande pra testar o hurry",
            "Linha 3 só de meme e bora deixar grande pra testar o hurry",
            "the fourth line, yep... e bora deixar grande pra testar o hurry",
            "5 e bora deixar grande pra testar o hurry aaaaaaaa"
        };


        private void Start()
        {
            _dialogueSystem = DialogueSystem.instance;
            _textArchitect = new TextArchitect(_dialogueSystem.dialogueContainer.dialogueText);
            _textArchitect.buildMethod = TextArchitect.BuildMethod.fade;
        }

        private void Update()
        {
            //for new line
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_textArchitect.isBuilding)
                {
                    //if (!_textArchitect.hurryUp)
                    //    _textArchitect.hurryUp = true;
                    //else
                    //    _textArchitect.ForceComplete();
                    _textArchitect.ForceComplete();
                }
                else
                    _textArchitect.Build(testingLines[Random.Range(0, testingLines.Length)]);
            }

            //for appending
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (_textArchitect.isBuilding)
                {
                    if (!_textArchitect.hurryUp)
                        _textArchitect.hurryUp = true;
                    else
                        _textArchitect.ForceComplete();
                }
                else
                    _textArchitect.Append(testingLines[Random.Range(0, testingLines.Length)]);
            }

        }
    }

}