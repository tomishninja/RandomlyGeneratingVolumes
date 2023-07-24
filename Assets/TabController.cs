using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class TabController : MonoBehaviour
{
    public GameObject[] objectsToHide;
    public GameObject tabPanel;
    private bool isHidden = false;
    private Vector3 tabPanelScale;
    private Vector3 ButtonScale;

    private void Start()
    {
        tabPanelScale = tabPanel.transform.localScale;
        ButtonScale = transform.localScale;
        //tabPanel.transform.localScale = Vector3.zero;
        HideGameObjects();
    }

    public void OnTabButtonClick()
    {
        if (isHidden)
        {
            isHidden = false;
            ShowGameObjects();

            // Show the hidden objects
            // ShowGameObjects();
            //tabPanel.SetActive(false);
            //tabPanel.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            //{
                isHidden = false;
                ShowGameObjects();
            //});
            //tabPanel.transform.DOMoveY(tabPanel.transform.position.y, 0.5f).SetEase(Ease.Linear);
        }
        else
        {
            isHidden = true;
            HideGameObjects();

            //HideGameObjects();
            //tabPanel.SetActive(true);
            //tabPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            //{
                isHidden = true;
                HideGameObjects();
            //});
        }
    }

    public void HideGameObjects()
    {
        for (int index = 0; index < objectsToHide.Length; index++)
        {
            objectsToHide[index].transform.DOScale(new Vector3(this.tabPanel.transform.localScale.x, this.transform.localScale.y, this.tabPanel.transform.localScale.z), 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                objectsToHide[index].SetActive(false);
            });

        }
    }

    public void ShowGameObjects()
    {
        for (int index = 0; index < objectsToHide.Length; index++)
        {
            objectsToHide[index].SetActive(true);
            objectsToHide[index].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }
}
