using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour {

    [SerializeField] private Image barImage;
    [SerializeField] private CuttingCounter cuttingCounter;

    private void Start() {
        cuttingCounter.OnProgressChanged += CuttingCounter_OnProgressChanged;

        barImage.fillAmount = 0;
        Hide();
    }

    private void CuttingCounter_OnProgressChanged(object sender, CuttingCounter.OnProgressChangedArgs e) {
        barImage.fillAmount = e.progressNormalized;

        if (barImage.fillAmount == 0f ||  barImage.fillAmount == 1f) {
            Hide();
        } else {
            Show();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
