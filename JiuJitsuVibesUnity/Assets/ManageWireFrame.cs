using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageWireFrame : MonoBehaviour {

    public GameObject[] pages;
	// Use this for initialization
	void Start () {
		for(int i = 1; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
	}

    public void LogIn()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 2)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
        
    }

    public void Register()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if(i != 1)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }
    public void ChooseModule()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 3)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }

    public void Progression()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 4)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }

    public void DrillPlan()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 5)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }

    public void BeltTestGameMode()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 6)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }

    public void SelectTopic()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 7)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }

    public void Game_Course()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 8)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }

    public void Quiz()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 9)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }
    public void WellDone()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (i != 10)
            {
                pages[i].SetActive(false);
            }
            else { pages[i].SetActive(true); }
        }
    }





    // Update is called once per frame
    void Update () {
		
	}
}
