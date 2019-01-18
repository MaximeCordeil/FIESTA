﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IATK;
using VRTK;
using VRTK.GrabAttachMechanics;
using System;
using System.Globalization;
using System.Security.Cryptography;
using DG.Tweening;
using UnityEditorInternal.VersionControl;

/// <summary>
/// Acts as a wrapper for IATK's visualisation script
/// </summary>
public class Chart : MonoBehaviour
{

    private Visualisation visualisation;
    private GameObject visualisationGameObject;
    private VRTK_InteractableObject interactableObject;
    private VRTK_BaseGrabAttach grabAttach;
    private BoxCollider boxCollider;
    private Rigidbody rigidbody;

    private Chart[,] splomCharts;  // Stored as 2D array
    private List<Chart> subCharts;  // Stored as 1D array
    private SPLOMButton[] splomButtons;

    private DisplayScreen displayScreen;

    private bool isPrototype = false;
    private bool isThrowing = false;
    private bool isDestroying = false;
    private bool isTouchingDisplayScreen = false;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private float deletionTimer = 0;

    #region VisualisationProperties

    public DataSource DataSource
    {
        get { return visualisation.dataSource; }
        set { visualisation.dataSource = value; }
    }

    private AbstractVisualisation.VisualisationTypes visualisationType;

    public AbstractVisualisation.VisualisationTypes VisualisationType
    {
        get { return visualisationType; }
        set
        {
            if (visualisationType != value)
            {
                visualisationType = value;

                switch (visualisationType)
                {
                    case AbstractVisualisation.VisualisationTypes.SCATTERPLOT:
                        SetAsScatterplot();
                        break;

                    case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                        SetAsScatterplotMatrix();
                        break;
                }
            }
        }
    }

    public AbstractVisualisation.GeometryType GeometryType
    {
        get { return visualisation.geometry; }
        set
        {
            visualisation.geometry = value;
            visualisation.updateViewProperties(AbstractVisualisation.PropertyType.GeometryType);

            switch (visualisationType)
            {
                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                    foreach (Chart chart in subCharts)
                        chart.GeometryType = value;
                    break;
            }
        }
    }

    public string XDimension
    {
        get { return visualisation.xDimension.Attribute; }
        set
        {
            visualisation.xDimension = value;
            visualisation.updateViewProperties(AbstractVisualisation.PropertyType.X);
            CenterVisualisation();
            SetColliderBounds();
        }
    }

    public string YDimension
    {
        get { return visualisation.yDimension.Attribute; }
        set
        {
            visualisation.yDimension = value;
            visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Y);
            CenterVisualisation();
            SetColliderBounds();
        }
    }

    public string ZDimension
    {
        get { return visualisation.zDimension.Attribute; }
        set
        {
            visualisation.zDimension = value;
            visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Z);
            CenterVisualisation();
            SetColliderBounds();
        }
    }

    public Color Color
    {
        get { return visualisation.colour; }
        set
        {
            visualisation.colour = value;
            visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Colour);

            switch (visualisationType)
            {
                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                    foreach (Chart chart in subCharts)
                        chart.Color = value;
                    break;
            }
        }
    }

    public float Size
    {
        get { return visualisation.size; }
        set
        {
            visualisation.size = value;
            visualisation.updateViewProperties(AbstractVisualisation.PropertyType.SizeValues);

            switch (visualisationType)
            {
                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                    foreach (Chart chart in subCharts)
                        chart.Size = value;
                    break;
            }
        }
    }

    public float Width
    {
        get { return visualisation.width; }
        set
        {
            visualisation.width = value;

            switch (visualisationType)
            {
                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT:
                    // Update the axis object with the length
                    GameObject axis = visualisation.theVisualizationObject.X_AXIS;
                    if (axis != null)
                    {
                        axis.GetComponent<Axis>().Length = value;
                        axis.GetComponent<Axis>().UpdateLength();
                    }

                    visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
                    ForceViewScale();
                    CenterVisualisation();
                    SetColliderBounds();
                    break;

                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                    ResizeAndPositionScatterplotMatrix();
                    break;
            }
        }
    }

    public float Height
    {
        get { return visualisation.height; }
        set
        {
            visualisation.height = value;

            switch (visualisationType)
            {
                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT:
                    // Update the axis object with the length
                    GameObject axis = visualisation.theVisualizationObject.Y_AXIS;
                    if (axis != null)
                    {
                        axis.GetComponent<Axis>().Length = value;
                        axis.GetComponent<Axis>().UpdateLength();
                    }

                    visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
                    ForceViewScale();
                    CenterVisualisation();
                    SetColliderBounds();
                    break;

                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                    ResizeAndPositionScatterplotMatrix();
                    break;
            }
        }
    }

    public float Depth
    {
        get { return visualisation.depth; }
        set
        {
            visualisation.depth = value;

            switch (visualisationType)
            {
                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT:
                    // Update the axis object with the length
                    GameObject axis = visualisation.theVisualizationObject.Z_AXIS;
                    if (axis != null)
                    {
                        axis.GetComponent<Axis>().Length = value;
                        axis.GetComponent<Axis>().UpdateLength();
                    }

                    visualisation.updateViewProperties(AbstractVisualisation.PropertyType.Scaling);
                    ForceViewScale();
                    CenterVisualisation();
                    SetColliderBounds();
                    break;

                case AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX:
                    ResizeAndPositionScatterplotMatrix();
                    break;
            }
        }
    }

    private int scatterplotMatrixSize = 3;
    public int ScatterplotMatrixSize
    {
        get { return scatterplotMatrixSize; }
        set
        {
            if (scatterplotMatrixSize < 2)
                value = 2;
            else if (scatterplotMatrixSize > DataSource.DimensionCount)
                value = DataSource.DimensionCount;

            // If value has changed, resize SPLOM
            if (scatterplotMatrixSize != value)
            {
                scatterplotMatrixSize = value;
                AdjustScatterplotMatrixSize();
            }
        }
    }

#endregion

    public void Initialise(CSVDataSource dataSource)
    {
        gameObject.tag = "Chart";

        visualisationGameObject = new GameObject("Visualisation");
        visualisationGameObject.transform.SetParent(transform);

        visualisation = visualisationGameObject.AddComponent<Visualisation>();

        DataSource = dataSource;
        displayScreen = GameObject.FindGameObjectWithTag("DisplayScreen").GetComponent<DisplayScreen>();
        
        // Set blank values
        visualisation.colourDimension = "Undefined";
        visualisation.sizeDimension = "Undefined";
        visualisation.linkingDimension = "Undefined";
        visualisation.colorPaletteDimension = "Undefined";

        // Add VRTK interactable scripts
        interactableObject = gameObject.AddComponent<VRTK_InteractableObject>();
        interactableObject.isGrabbable = true;
        grabAttach = gameObject.AddComponent<VRTK_ChildOfControllerGrabAttach>();
        interactableObject.grabAttachMechanicScript = grabAttach;
        interactableObject.grabAttachMechanicScript.precisionGrab = true;

        // Subscribe to events
        interactableObject.InteractableObjectGrabbed += ChartGrabbed;
        interactableObject.InteractableObjectUngrabbed += ChartUngrabbed;

        // Add collider
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        // Configure rigidbody
        rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
    }

    public void ForceUpdate()
    {
        visualisation.updateProperties();
    }

    private void SetAsScatterplot()
    {
        // Enable the visualisation
        visualisationGameObject.SetActive(true);

        // Enable this collider
        boxCollider.enabled = transform;
        
        // Destroy scatterplot matrix gameobjects
        for (int i = 0; i < splomCharts.GetLength(0); i++)
            for (int j = 0; j < splomCharts.GetLength(1); j++)
                Destroy(splomCharts[i, j]);

        visualisation.visualisationType = AbstractVisualisation.VisualisationTypes.SCATTERPLOT;
        visualisation.CreateVisualisation(visualisation.visualisationType);
    }

    private void SetAsScatterplotMatrix()
    {
        // Disable the visualisation
        visualisationGameObject.SetActive(false);

        // Disable this collider
        boxCollider.enabled = false;

        // Create scatterplot matrix gameobjects
        int nbDimensions = DataSource.DimensionCount;
        splomCharts = new Chart[nbDimensions, nbDimensions];
        subCharts = new List<Chart>();
        splomButtons = new SPLOMButton[nbDimensions];

        AdjustScatterplotMatrixSize();

        ResizeAndPositionScatterplotMatrix();
    }

    private void ScatterplotMatrixDimensionChanged(SPLOMButton button)
    {
        // Find which index the button belongs to (along the diagonal)
        int index = Array.IndexOf(splomButtons, button);

        // Change y-axis of charts along SPLOM's horizontal
        for (int i = 0; i < scatterplotMatrixSize; i++)
        {
            if (splomCharts[i, index].tag == "Chart")
            {
                splomCharts[i, index].GetComponent<Chart>().YDimension = button.Text;
                splomCharts[i, index].GetComponent<Chart>().ForceUpdate();
            }
        }

        // Change x-axis of charts along SPLOM's vertical
        for (int i = 0; i < scatterplotMatrixSize; i++)
        {
            if (splomCharts[index, i].tag == "Chart")
            {
                splomCharts[index, i].GetComponent<Chart>().XDimension = button.Text;
                splomCharts[index, i].GetComponent<Chart>().ForceUpdate();
            }
        }
    }

    private void AdjustScatterplotMatrixSize()
    {
        // Create scatterplot matrix gameobjects
        int nbDimensions = DataSource.DimensionCount;

        for (int i = 0; i < nbDimensions; i++)
        {
            for (int j = 0; j < nbDimensions; j++)
            {
                // Only add/modify a subchart if [i,j] are smaller than the SPLOM's size
                if (i < scatterplotMatrixSize && j < scatterplotMatrixSize)
                {
                    Chart subChart = splomCharts[i, j];
                    // Only create an object if there wasn't already one
                    if (subChart == null)
                    {
                        // If not along the diagonal, create a chart
                        if (i != j)
                        {
                            subChart = ChartManager.Instance.CreateVisualisation(DataSource[i].Identifier + ", " + DataSource[j].Identifier);

                            subChart.VisualisationType = AbstractVisualisation.VisualisationTypes.SCATTERPLOT;
                            subChart.GeometryType = AbstractVisualisation.GeometryType.Points;
                            // Get the x and y dimension from the splom button if it exists, otherwise use default
                            subChart.XDimension = (splomButtons[i] != null) ? splomButtons[i].Text : DataSource[i].Identifier;
                            subChart.YDimension = (splomButtons[j] != null) ? splomButtons[j].Text : DataSource[j].Identifier;
                            subChart.Color = Color;
                            subChart.ForceUpdate();
                            subChart.SetAsPrototype();

                            splomCharts[i, j] = subChart;
                            subChart.transform.SetParent(transform);
                            subCharts.Add(subChart);
                        }
                        // If along the diagonal, create a blank chart (only axis labels with no geometry or collider) and a SPLOM button
                        else
                        {
                            subChart = ChartManager.Instance.CreateVisualisation(DataSource[i].Identifier + ", " + DataSource[j].Identifier);
                            // Get the x and y dimension from the splom button if it exists, otherwise use default
                            subChart.XDimension = (splomButtons[i] != null) ? splomButtons[i].Text : DataSource[i].Identifier;
                            subChart.YDimension = (splomButtons[j] != null) ? splomButtons[j].Text : DataSource[j].Identifier;
                            subChart.GetComponent<Collider>().enabled = false;
                            subChart.transform.SetParent(transform);
                            splomCharts[i, j] = subChart;

                            GameObject go = Instantiate((GameObject)Resources.Load("SPLOMButton"));
                            SPLOMButton button = go.GetComponent<SPLOMButton>();
                            button.ButtonClicked.AddListener(ScatterplotMatrixDimensionChanged);
                            button.Text = DataSource[i].Identifier;
                            splomButtons[i] = button;
                            go.transform.SetParent(transform);
                        }
                    }

                    // Hide the axis for all but the charts along the edge
                    bool isAlongLeft = (i == 0);
                    bool isAlongBottom = (j == scatterplotMatrixSize - 1);
                    GameObject xAxis = subChart.visualisation.theVisualizationObject.X_AXIS;
                    GameObject yAxis = subChart.visualisation.theVisualizationObject.Y_AXIS;

                    if (!isAlongLeft && !isAlongBottom)
                    {
                        xAxis.SetActive(false);
                        yAxis.SetActive(false);
                    }
                    else if (isAlongLeft && !isAlongBottom)
                    {
                        xAxis.SetActive(false);
                        yAxis.SetActive(true);
                    }
                    else if (!isAlongLeft && isAlongBottom)
                    {
                        xAxis.SetActive(true);
                        yAxis.SetActive(false);
                    }
                    else
                    {
                        xAxis.SetActive(true);
                        yAxis.SetActive(true);
                    }
                }
                // If it is larger, delete any charts if there were any
                else
                {
                    Chart chart = splomCharts[i, j];

                    if (chart != null)
                    {
                        subCharts.Remove(chart.GetComponent<Chart>());
                        ChartManager.Instance.RemoveVisualisation(chart.GetComponent<Chart>());
                    }
                }
            }
        }
        
        // Remove any extra splom buttons
        for (int i = scatterplotMatrixSize; i < splomButtons.Length; i++)
        {
            if (splomButtons[i] != null)
            {
                Destroy(splomButtons[i].gameObject);
                splomButtons[i] = null;
            }
        }

        ResizeAndPositionScatterplotMatrix();
    }

    private void ResizeAndPositionScatterplotMatrix()
    {
        if (visualisationType == AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX && splomCharts != null)
        {
            for (int i = 0; i < scatterplotMatrixSize; i++)
            {
                // Resize splom button
                float w = Width / scatterplotMatrixSize;
                float h = Height / scatterplotMatrixSize;
                float d = Depth / scatterplotMatrixSize;
                float x = -0.5f + w * i;
                float y = 0.5f - h * i;

                SPLOMButton splomButton = splomButtons[i];

                splomButton.transform.localPosition = new Vector3(x, y, 0);
                splomButton.transform.rotation = transform.rotation;
                splomButton.transform.localScale = new Vector3(w * 0.9f, h * 0.9f, 0.05f);

                // Resize charts
                for (int j = 0; j < scatterplotMatrixSize; j++)
                {
                    y = 0.5f - h * j;

                    Chart chart = splomCharts[i, j];
                        
                    chart.Width = w * 0.75f;
                    chart.Height = h * 0.75f;
                    chart.Depth = d * 0.75f;
                    chart.transform.localPosition = new Vector3(x, y, 0);
                    chart.transform.rotation = transform.rotation;
                }
            }
        }
    }


    private void ChartGrabbed(object sender, InteractableObjectEventArgs e)
    {
        if (isPrototype)
        {
            originalPos = transform.position;
            originalRot = transform.rotation;
        }

        rigidbody.isKinematic = false;
        InteractionsManager.Instance.GrabbingStarted();  // TODO: FIX
    }

    private void ChartUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        InteractionsManager.Instance.GrabbingFinished(); // TODO: FIX

        // Animate the work shelf prototype back to its position
        if (isPrototype)
        {
            rigidbody.isKinematic = true;

            AnimateTowards(originalPos, originalRot, 0.2f);
        }
        else
        {
            // Check to see if the chart was thrown
            Vector3 velocity = VRTK_DeviceFinder.GetControllerVelocity(VRTK_ControllerReference.GetControllerReference(e.interactingObject));
            float speed = velocity.magnitude;

            if (speed > 2.5f)
            {
                rigidbody.useGravity = true;
                isThrowing = true;
                deletionTimer = 0;
            }
            else
            {
                rigidbody.isKinematic = true;

                // If it wasn't thrown, check to see if it is being placed on the display screen
                if (isTouchingDisplayScreen)
                {
                    AttachToDisplayScreen();
                }
            }
        }
    }

    private void OnDestroy()
    {
        //Unsubscribe to events
        interactableObject.InteractableObjectGrabbed -= ChartGrabbed;
        interactableObject.InteractableObjectUngrabbed -= ChartUngrabbed;
    }

    private void Update()
    {
        if (isPrototype && interactableObject.IsGrabbed())
        {
            if (Vector3.Distance(transform.position, originalPos) > 0.25f)
            {
                // Create a duplicate of this visualisation
                Chart dupe = ChartManager.Instance.DuplicateVisualisation(this);

                VRTK_InteractTouch interactTouch = interactableObject.GetGrabbingObject().GetComponent<VRTK_InteractTouch>();
                VRTK_InteractGrab interactGrab = interactableObject.GetGrabbingObject().GetComponent<VRTK_InteractGrab>();
                
                // Drop this visualisation (it wil return automatically)
                interactGrab.ForceRelease();

                // Grab the duplicate
                interactTouch.ForceTouch(dupe.gameObject);
                interactGrab.AttemptGrab();
            }
        }
        else if (isThrowing)
        {
            if (1 < deletionTimer)
            {
                isThrowing = false;
                isDestroying = true;
            }
            else
            {
                deletionTimer += Time.deltaTime;
            }
        }
        else if (isDestroying)
        {
            float size = transform.localScale.x;
            size -= 0.005f;

            if (size > 0)
            {
                transform.localScale = Vector3.one * size;
            }
            else
            {
                ChartManager.Instance.RemoveVisualisation(this);
                Destroy(gameObject);
            }
        }
    }

    public void SetAsPrototype()
    {
        isPrototype = true;
    }


    private void ForceViewScale()
    {
        foreach (View view in visualisation.theVisualizationObject.viewList)
        {
            view.transform.localScale = new Vector3(
                visualisation.width,
                visualisation.height,
                visualisation.depth
            );
        }
    }

    /// <summary>
    /// Sets the size of the collider based on the size and dimensions stored in the Visualisation. This should be called whenever a dimension is added/changed or when
    /// the size is changed.
    /// </summary>
    private void SetColliderBounds()
    {
        float width = visualisation.width;
        float height = visualisation.height;
        float depth = visualisation.depth;

        string x = visualisation.xDimension.Attribute;
        string y = visualisation.yDimension.Attribute;
        string z = visualisation.zDimension.Attribute;

        //// Calculate center
        //float xCenter = (x != "Undefined") ? width / 2 : 0;
        //float yCenter = (y != "Undefined") ? height / 2 : 0;
        //float zCenter = (z != "Undefined") ? depth/ 2 : 0;

        // Calculate size
        float xSize = (x != "Undefined") ? width + 0.15f : 0.1f;
        float ySize = (y != "Undefined") ? height + 0.15f : 0.1f;
        float zSize = (z != "Undefined") ? depth + 0.15f : 0.1f;

        //boxCollider.center = new Vector3(xCenter, yCenter, zCenter);
        boxCollider.size = new Vector3(xSize, ySize, zSize);
    }

    private void CenterVisualisation()
    {
        float x = (XDimension != "Undefined") ? -Width / 2 : 0;
        float y = (YDimension != "Undefined") ? -Height / 2 : 0;
        float z = (ZDimension != "Undefined") ? -Depth / 2 : 0;

        visualisationGameObject.transform.DOLocalMove(new Vector3(x, y, z), 0.1f).SetEase(Ease.OutCubic);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DisplayScreen")
        {
            isTouchingDisplayScreen = true;

            // If the chart was thrown at the screen, attach it to the screen
            if (isThrowing)
            {
                isThrowing = false;
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                AttachToDisplayScreen();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "DisplayScreen")
        {
            isTouchingDisplayScreen = false;
        }
    }

    private void AttachToDisplayScreen()
    {
        Vector3 pos = displayScreen.CalculatePositionOnScreen(this);
        Quaternion rot = displayScreen.CalculateRotationOnScreen(this);

        AnimateTowards(pos, rot, 0.2f);
    }

    public void AnimateTowards(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        rigidbody.DOMove(targetPos, duration).SetEase(Ease.OutQuint);
        rigidbody.DORotate(targetRot.eulerAngles, duration).SetEase(Ease.OutQuint);
    }
}
