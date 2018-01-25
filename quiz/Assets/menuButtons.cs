using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class menuButtons : MonoBehaviour {

    public GameObject apie;
    public GameObject menu;

	// Use this for initialization
	void Start () {
        apie.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Pradeti()
    {
        Application.LoadLevel("game");
    }

    public void Apie()
    {
        apie.SetActive(true);
        menu.SetActive(false);
    }

    public void Knyga()
    {
        Application.OpenURL("http://www.mokslofestivalis.eu/wp-content/uploads/2015/06/reiksmingiausios_idejos.pdf");
    }

    public void Iseiti()
    {
        Application.Quit();
    }

    public void Menu()
    {
        menu.SetActive(true);
        apie.SetActive(false);
    }
}
