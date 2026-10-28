using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InsufficientCredits : MonoBehaviour
{
    [SerializeField] private GameObject textMeshProObj;
    [SerializeField] private float flashOnInterval;
    [SerializeField] private float flashOffInterval;
    private float currentFlashTime = 0;

    private void Update()
    {
        bool isActive = textMeshProObj.activeSelf;

        if (isActive) {
            if (currentFlashTime < flashOnInterval) {
                currentFlashTime += Time.deltaTime;
            } else {
                currentFlashTime = 0;
                textMeshProObj.SetActive(false);
            }
        } else {
            if (currentFlashTime < flashOffInterval) {
                currentFlashTime += Time.deltaTime;
            } else {
                currentFlashTime = 0;
                textMeshProObj.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }


}
