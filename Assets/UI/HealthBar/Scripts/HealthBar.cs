using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
	public TextMeshProUGUI HealthValue;
	public Slider slider;
	public Gradient gradient;
	public Image fill;

	public void SetMaxHealth()
	{
		slider.maxValue = 100;
		slider.value = 100;
		fill.color = gradient.Evaluate(1f);
	}

    public void SetHealth(int health)
	{
		slider.value = health;

		fill.color = gradient.Evaluate(slider.normalizedValue);
	}

	public void SetHealthValue(int currentHealh, int maxHealth)
	{
		string maxHealthStr = maxHealth.ToString();
		string currentHealthStr = currentHealh.ToString();
		HealthValue.text = currentHealthStr + " / " + maxHealthStr;
	}
}
