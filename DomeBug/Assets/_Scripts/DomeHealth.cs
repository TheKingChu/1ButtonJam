using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DomeHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthSlider;
    private Image fillImage;
    public float flashDuration = 0.2f;
    public Color flashColor = Color.red;
    private Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if(healthSlider != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
            originalColor = fillImage.color;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthSlider.value = currentHealth;
        if(fillImage != null)
        {
            StartCoroutine(Flash());
        }
        if(currentHealth <= 0)
        {
            currentHealth = 0;
            GameOver();
        }
    }

    private IEnumerator Flash()
    {
        fillImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        fillImage.color = originalColor;
    }

    private void GameOver()
    {
        //add gameover
        Debug.Log("game over");
        SceneManager.LoadScene("menu");
    }
}
