using System;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using Yarn.Unity.Samples;

public class CustomerDialoguePresenter : DialoguePresenterBase
{
    [SerializeField] TMP_Text text;
    [SerializeField] CanvasGroup canvasGroup;
    
    [Header("Timing")]
    [SerializeField] int millisecondsPerCharacter = 75;
    [SerializeField] float minDuration = 1.5f;
    
    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        if (text == null)
        {
            return;
        }

        canvasGroup.alpha = 1;
        var lineText = line.TextWithoutCharacterName.Text;

        text.text = lineText;

        var durationMilliseconds = Mathf.Max(lineText.Length * millisecondsPerCharacter, this.minDuration * 1000);

        try
        {
            text.enabled = true;
            await YarnTask.Delay(TimeSpan.FromMilliseconds(durationMilliseconds), token.NextContentToken);
            text.enabled = false;
            // await YarnTask.Delay(TimeSpan.FromMilliseconds(this.delayAfterLines), token.NextContentToken);
        }
        catch (OperationCanceledException)
        {
            // Line cancelled, nothing to do
        }
        finally
        {
            text.enabled = false;
        }
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
        return YarnTask.CompletedTask;
    }
    public override YarnTask OnDialogueCompleteAsync()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
        return YarnTask.CompletedTask;
    }
}
