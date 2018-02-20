using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PropertyController : MonoBehaviour
{
    public Text _Name;
    public GameObject MenuPanel;
    public GameObject SearchResultsPanel;
    public GameObject objectOptionsPanel;
    public GameObject positionArrowPanel;
    public GameObject rotationArrowPanel;
    public GameObject scaleArrowPanel;
    public InputField[] _Position;
    public InputField[] _Rotation;
    public InputField[] _Scale;
    public Toggle MaintainScale;
    public Toggle ObeyGravity;

    private Vector3 pos;
    private Vector3 rot;
    private Vector3 scale;
    private userAsset model;
    private bool active;
    private GameObject selected;
    private string inputSelected;

    private bool RTriggerHeld;
    private bool LTriggerHeld;

    private double LDownTime;
    private double RDownTime;
    private double triggerTime = 10.0; // 10 milliseconds
    private bool RTriggerDown;
    private bool LTriggerDown;

    private Vector3 rayHitPoint;

    private Color normalButton = new Color(0.3f, 0.3f, 0.3f);
    private Color highlightButton = new Color(0.6f, 0.6f, 0.6f);
    private Color selectButton = new Color(0.6f, 0.6f, 10f);

    private Color normalInput = new Color(1.0f, 1.0f, 1.0f);
    private Color highlightInput = new Color(0.8f, 0.8f, 0.8f);
    private Color selectInput = new Color(0.6f, 0.6f, 10f);

    // Use this for initialization
    void Start()
    {
        active = false;
        inputSelected = "";
        selected = null;

        objectOptionsPanel.SetActive(false);
        positionArrowPanel.SetActive(false);
        rotationArrowPanel.SetActive(false);
        scaleArrowPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RTriggerDown = getRightTriggerDown();
        LTriggerDown = getLeftTriggerDown();

        if (RTriggerDown)
            selectModel();
        else if ((Input.GetButtonDown("BButton") || Input.GetButtonDown("YButton")) && active)
            deSelectModel();
        if (selected != null)
        {
            updateButtonColors(objectOptionsPanel.transform);
            updateFields(selected);
        }
    }

    void updateFields(GameObject Object)
    {
        pos = Object.transform.position;
        rot = Object.transform.localEulerAngles;
        scale = Object.transform.localScale;
        model = Object.GetComponent<userAsset>();

        if (inputSelected == "xPosition")
        {
            _Position[0].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(true);
            rotationArrowPanel.SetActive(false);
            scaleArrowPanel.SetActive(false);
            mapControls(positionArrowPanel, 0, "position");
        }
        else if (inputSelected == "yPosition")
        {
            _Position[1].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(true);
            rotationArrowPanel.SetActive(false);
            scaleArrowPanel.SetActive(false);
            mapControls(positionArrowPanel, 1, "position");
        }
        else if (inputSelected == "zPosition")
        {
            _Position[2].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(true);
            rotationArrowPanel.SetActive(false);
            scaleArrowPanel.SetActive(false);
            mapControls(positionArrowPanel, 2, "position");
        }
        else if (inputSelected == "xRotation")
        {
            _Rotation[0].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(false);
            rotationArrowPanel.SetActive(true);
            scaleArrowPanel.SetActive(false);
            mapControls(rotationArrowPanel, 0, "rotation");
        }
        else if (inputSelected == "yRotation")
        {
            _Rotation[1].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(false);
            rotationArrowPanel.SetActive(true);
            scaleArrowPanel.SetActive(false);
            mapControls(rotationArrowPanel, 1, "rotation");
        }
        else if (inputSelected == "zRotation")
        {
            _Rotation[2].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(false);
            rotationArrowPanel.SetActive(true);
            scaleArrowPanel.SetActive(false);
            mapControls(rotationArrowPanel, 2, "rotation");
        }
        else if (inputSelected == "xScale")
        {
            _Scale[0].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(false);
            rotationArrowPanel.SetActive(false);
            scaleArrowPanel.SetActive(true);
            mapControls(scaleArrowPanel, 0, "scale");
        }
        else if (inputSelected == "yScale")
        {
            _Scale[1].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(false);
            rotationArrowPanel.SetActive(false);
            scaleArrowPanel.SetActive(true);
            mapControls(scaleArrowPanel, 1, "scale");
        }
        else if (inputSelected == "zScale")
        {
            _Scale[2].gameObject.GetComponent<Image>().color = new Color(0.61f, 0.66f, 0.79f, 1);
            positionArrowPanel.SetActive(false);
            rotationArrowPanel.SetActive(false);
            scaleArrowPanel.SetActive(true);
            mapControls(scaleArrowPanel, 2, "scale");
        }

        if (inputSelected != "xPosition")
        {
            _Position[0].text = Object.transform.position.x.ToString("0.00");
            _Position[0].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "yPosition")
        {
            _Position[1].text = Object.transform.position.y.ToString("0.0");
            _Position[1].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "zPosition")
        {
            _Position[2].text = Object.transform.position.z.ToString("0.00");
            _Position[2].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "xRotation")
        {
            _Rotation[0].text = Object.transform.localEulerAngles.x.ToString("0.00");
            _Rotation[0].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "yRotation")
        {
            _Rotation[1].text = Object.transform.localEulerAngles.y.ToString("0.00");
            _Rotation[1].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "zRotation")
        {
            _Rotation[2].text = Object.transform.localEulerAngles.z.ToString("0.00");
            _Rotation[2].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "xScale")
        {
            _Scale[0].text = Object.transform.localScale.x.ToString("0.00");
            _Scale[0].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "yScale")
        {
            _Scale[1].text = Object.transform.localScale.y.ToString("0.00");
            _Scale[1].gameObject.GetComponent<Image>().color = Color.white;
        }
        if (inputSelected != "zScale")
        {
            _Scale[2].text = Object.transform.localScale.z.ToString("0.00");
            _Scale[2].gameObject.GetComponent<Image>().color = Color.white;
        }


        RaycastHit seen;

        Vector3 rotation = transform.localEulerAngles;
        rotation.x += 15;

        Vector3 forwardVector = Quaternion.Euler(rotation) * Vector3.forward;

        Ray raydirection = new Ray(transform.position, forwardVector);
        if (Physics.Raycast(raydirection, out seen))
        {
            rayHitPoint = seen.point;

            if ((RTriggerDown))
            {
                if (seen.collider.tag == "Button")
                {
                    seen.collider.gameObject.GetComponent<Button>().onClick.Invoke();
                }
                else if (seen.collider.tag == "Input")
                {
                    inputSelected = seen.collider.name;
                    Debug.Log(inputSelected);
                }
                else if (seen.collider.tag == "Check")
                {
                    seen.collider.gameObject.GetComponent<Toggle>().isOn =
                        !seen.collider.gameObject.GetComponent<Toggle>().isOn;

                    if (seen.collider.gameObject.name == "Gravity")
                    {
                        model.Gravity = ! model.Gravity;
                        model.Physics();
                    }
                    if (seen.collider.gameObject.name == "Maintain")
                    {
                        model.Maintain = !model.Maintain;
                        if(model.Maintain)
                            seen.collider.gameObject.GetComponent<Image>().color = new Color(0, 191, 0);
                        else
                            seen.collider.gameObject.GetComponent<Image>().color = new Color(191, 0, 0);
                    }
                }
            }
        }
    }

    void mapControls(GameObject panel, int index, string type)
    {
        pos = selected.transform.position;
        rot = selected.transform.localEulerAngles;
        scale = selected.transform.localScale;
        model = selected.GetComponent<userAsset>();
        Button[] buttons = panel.transform.GetComponentsInChildren<Button>();

        buttons[0].onClick.RemoveAllListeners();
        buttons[1].onClick.RemoveAllListeners();
        buttons[2].onClick.RemoveAllListeners();
        buttons[3].onClick.RemoveAllListeners();

        buttons[0].onClick.AddListener(delegate {
            if (type == "position")
            {
                if (index == 0)
                {
                    pos.x = selected.transform.position.x - 1.0f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    pos.y = selected.transform.position.y - 1.0f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    pos.z = selected.transform.position.z - 1.0f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.z.ToString("0.00");
                }
            }
            else if (type == "rotation")
            {
                if (index == 0)
                {
                    rot.x = selected.transform.localEulerAngles.x - 1.0f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    rot.y = selected.transform.localEulerAngles.y - 1.0f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    rot.z = selected.transform.localEulerAngles.z - 1.0f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.z.ToString("0.00");
                }
            }
            else if (type == "scale")
            {
                if (index == 0)
                {
                    scale.x = selected.transform.localScale.x - 1.0f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    scale.y = selected.transform.localScale.y - 1.0f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    scale.z = selected.transform.localScale.z - 1.0f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.z.ToString("0.00");
                }
            }
        });
        buttons[1].onClick.AddListener(delegate {
            if (type == "position")
            {
                if (index == 0)
                {
                    pos.x = selected.transform.position.x - 0.1f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    pos.y = selected.transform.position.y - 0.1f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    pos.z = selected.transform.position.z - 0.1f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.z.ToString("0.00");
                }
            }
            else if (type == "rotation")
            {
                if (index == 0)
                {
                    rot.x = selected.transform.localEulerAngles.x - 0.1f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    rot.y = selected.transform.localEulerAngles.y - 0.1f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    rot.z = selected.transform.localEulerAngles.z - 0.1f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.z.ToString("0.00");
                }
            }
            else if (type == "scale")
            {
                if (index == 0)
                {
                    scale.x = selected.transform.localScale.x - 0.1f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    scale.y = selected.transform.localScale.y - 0.1f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    scale.z = selected.transform.localScale.z - 0.1f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.z.ToString("0.00");
                }
            }
        });
        buttons[2].onClick.AddListener(delegate {
            if (type == "position")
            {
                if (index == 0)
                {
                    pos.x = selected.transform.position.x + 0.1f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    pos.y = selected.transform.position.y + 0.1f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    pos.z = selected.transform.position.z + 0.1f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.z.ToString("0.00");
                }
            }
            else if (type == "rotation")
            {
                if (index == 0)
                {
                    rot.x = selected.transform.localEulerAngles.x + 0.1f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    rot.y = selected.transform.localEulerAngles.y + 0.1f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    rot.z = selected.transform.localEulerAngles.z + 0.1f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.z.ToString("0.00");
                }
            }
            else if (type == "scale")
            {
                if (index == 0)
                {
                    scale.x = selected.transform.localScale.x + 0.1f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    scale.y = selected.transform.localScale.y + 0.1f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    scale.z = selected.transform.localScale.z + 0.1f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.z.ToString("0.00");
                }
            }
        });
        buttons[3].onClick.AddListener(delegate {
            if (type == "position")
            {
                if (index == 0)
                {
                    pos.x = selected.transform.position.x + 1.0f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    pos.y = selected.transform.position.y + 1.0f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    pos.z = selected.transform.position.z + 1.0f;
                    model.Position(pos);
                    _Position[index].text = selected.transform.position.z.ToString("0.00");
                }
            }
            else if (type == "rotation")
            {
                if (index == 0)
                {
                    rot.x = selected.transform.localEulerAngles.x + 1.0f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    rot.y = selected.transform.localEulerAngles.y + 1.0f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    rot.z = selected.transform.localEulerAngles.z + 1.0f;
                    model.Rotation(rot);
                    _Rotation[index].text = selected.transform.localEulerAngles.z.ToString("0.00");
                }
            }
            else if (type == "scale")
            {
                if (index == 0)
                {
                    scale.x = selected.transform.localScale.x + 1.0f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.x.ToString("0.00");
                }
                else if (index == 1)
                {
                    scale.y = selected.transform.localScale.y + 1.0f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.y.ToString("0.00");
                }
                else if (index == 2)
                {
                    scale.z = selected.transform.localScale.z + 1.0f;
                    model.Scale(scale);
                    _Scale[index].text = selected.transform.localScale.z.ToString("0.00");
                }
            }
        });
    }

    private bool CheckBoxCollision(BoxCollider collider, Vector3 point)
    {
        Vector3 posToCheck = point;
        Vector3 offset = collider.bounds.center - posToCheck;
        posToCheck = point + offset * 0.25f;
        offset = collider.bounds.center - posToCheck;
        Ray inputRay = new Ray(posToCheck, offset.normalized);
        RaycastHit rHit;

        return !collider.Raycast(inputRay, out rHit, offset.magnitude * 1.1f);
    }

    private void updateButtonColors(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform element = parent.GetChild(i);
            updateButtonColorsInChildren(element);
            updateButtonColors(element);
        }
    }

    private void updateButtonColorsInChildren(Transform parent)
    {
        GameObject element = parent.gameObject;
        if (element.tag == "Input")
        {
            InputField input = element.GetComponent<InputField>();

            ColorBlock cb = input.colors;
            if (CheckBoxCollision(input.GetComponent<BoxCollider>(), rayHitPoint))
            {
                if (RTriggerDown)
                {
                    cb.normalColor = selectInput;
                }
                else
                {
                    cb.normalColor = highlightInput;
                }
            }
            else
            {
                cb.normalColor = normalInput;
            }
            input.colors = cb;
        }
        else if (element.tag == "Button")
        {
            Button currentButton = element.GetComponent<Button>();
            ColorBlock cb = currentButton.colors;
            if (CheckBoxCollision(currentButton.GetComponent<BoxCollider>(), rayHitPoint))
            {
                if (RTriggerDown)
                {
                    cb.normalColor = selectButton;
                }
                else
                {
                    cb.normalColor = highlightButton;
                }
            }
            else
            {
                cb.normalColor = normalButton;
            }
            currentButton.colors = cb;
        }
        if (element.tag == "Check")
        {
            Toggle toggle = element.GetComponent<Toggle>();

            ColorBlock cb = toggle.colors;
            if (CheckBoxCollision(toggle.GetComponent<BoxCollider>(), rayHitPoint))
            {
                if (RTriggerDown)
                {
                    cb.normalColor = selectInput;
                }
                else
                {
                    cb.normalColor = highlightInput;
                }
            }
            else
            {
                cb.normalColor = normalInput;
            }
            toggle.colors = cb;
        }
    }

    void selectModel()
    {
        RaycastHit seen;

        Vector3 rotation = transform.localEulerAngles;
        rotation.x += 15;

        Vector3 forwardVector = Quaternion.Euler(rotation) * Vector3.forward;

        Ray raydirection = new Ray(transform.position, forwardVector);
        if (Physics.Raycast(raydirection, out seen))
        {
            Debug.Log(seen.collider.name);
            if (seen.collider.gameObject.transform.parent.gameObject.GetComponent<userAsset>() != null)
            {
                if (active)
                {
                    deSelectModel();
                }
                active = true;
                selected = seen.collider.gameObject.transform.parent.gameObject;
                objectOptionsPanel.SetActive(true);
                SearchResultsPanel.SetActive(false);
                MenuPanel.SetActive(false);

                pos = selected.transform.position;
                rot = selected.transform.localEulerAngles;
                scale = selected.transform.localScale;
                model = selected.GetComponent<userAsset>();

                MaintainScale.isOn = model.Maintain;
                ObeyGravity.isOn = model.Gravity;
                _Name.text = "Name: " + seen.collider.name;

                model.Physics();

                _Position[0].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Position[0].text, out pos.x);
                    model.Position(pos);
                });
                _Position[1].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Position[1].text, out pos.y);
                    model.Position(pos);
                });
                _Position[2].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Position[2].text, out pos.z);
                    model.Position(pos);
                });
                _Rotation[0].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Rotation[0].text, out rot.x);
                    model.Rotation(rot);
                });
                _Rotation[1].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Rotation[1].text, out rot.y);
                    model.Rotation(rot);
                });
                _Rotation[2].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Rotation[2].text, out rot.z);
                    model.Rotation(rot);
                });
                _Scale[0].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Scale[0].text, out scale.x);
                    model.Scale(scale);
                });
                _Scale[1].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Scale[1].text, out scale.y);
                    model.Scale(scale);
                });
                _Scale[2].onEndEdit.AddListener(delegate
                {
                    float.TryParse(_Scale[2].text, out scale.z);
                    model.Scale(scale);
                });
            }
        }
    }

    public void deSelectModel()
    {
        selected = null;
        inputSelected = "";
        objectOptionsPanel.SetActive(false);
        positionArrowPanel.SetActive(false);
        rotationArrowPanel.SetActive(false);
        scaleArrowPanel.SetActive(false);

        active = false;
        _Position[0].onEndEdit.RemoveAllListeners();
        _Position[1].onEndEdit.RemoveAllListeners();
        _Position[2].onEndEdit.RemoveAllListeners();
        _Rotation[0].onEndEdit.RemoveAllListeners();
        _Rotation[1].onEndEdit.RemoveAllListeners();
        _Rotation[2].onEndEdit.RemoveAllListeners();
        _Scale[0].onEndEdit.RemoveAllListeners();
        _Scale[1].onEndEdit.RemoveAllListeners();
        _Scale[2].onEndEdit.RemoveAllListeners();
    }

    private bool getRightTriggerDown()
    {
        float pressure = Input.GetAxisRaw("RightTrigger");
        bool down = pressure > 0.2f;
        if (down)
        {
            if (RTriggerHeld)
            {
                if (NowMilliseconds() - RDownTime < triggerTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                RDownTime = NowMilliseconds();
                RTriggerHeld = true;
                return true;
            }
        }
        else
        {
            RTriggerHeld = false;
            return false;
        }
    }

    private bool getLeftTriggerDown()
    {
        float pressure = Input.GetAxisRaw("LeftTrigger");
        bool down = pressure > 0.2f;
        if (down)
        {
            if (LTriggerHeld)
            {
                if (NowMilliseconds() - LDownTime < triggerTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LDownTime = NowMilliseconds();
                LTriggerHeld = true;
                return true;
            }
        }
        else
        {
            LTriggerHeld = false;
            return false;
        }
    }

    private double NowMilliseconds()
    {
        return (System.DateTime.UtcNow -
                new System.DateTime(1970, 1, 1, 0, 0, 0,
                    System.DateTimeKind.Utc)).TotalMilliseconds;
    }
}
