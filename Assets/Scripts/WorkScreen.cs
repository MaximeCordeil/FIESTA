﻿using System.Collections;
using System.Collections.Generic;
using IATK;
using UnityEngine;

public class WorkScreen : MonoBehaviour
{
    private DataSource dataSource;

    private Chart scatterplot;
    private Chart scatterplotMatrix;
    private Chart facet;

    [SerializeField]
    private Transform scatterplotTransform;
    [SerializeField]
    private Transform scatterplotMatrixTransform;
    [SerializeField]
    private Transform facetTransform;

    [SerializeField]
    private List<Menu> scatterplotButtons;
    [SerializeField]
    private List<GameObject> scatterplotMatrixButtons;
    [SerializeField]
    private List<GameObject> facetButtons;

    private void Start()
    {
        if (dataSource == null)
            dataSource = ChartManager.Instance.dataSource;

        if (scatterplotButtons == null)
            scatterplotButtons = new List<Menu>();

        if (scatterplotMatrixButtons == null)
            scatterplotMatrixButtons = new List<GameObject>();
        
        if (facetButtons == null)
            facetButtons = new List<GameObject>();

        // Configure scatterplot
        scatterplot = ChartManager.Instance.CreateVisualisation("WorkscreenScatterplot");
        scatterplot.VisualisationType = AbstractVisualisation.VisualisationTypes.SCATTERPLOT;
        scatterplot.GeometryType = AbstractVisualisation.GeometryType.Points;
        scatterplot.XDimension = dataSource[0].Identifier;
        scatterplot.YDimension = dataSource[0].Identifier;
        scatterplot.Width = 0.6f;
        scatterplot.Height = 0.6f;
        scatterplot.Depth = 0.6f;
        scatterplot.Color = Color.white;
        scatterplot.transform.position = scatterplotTransform.position;
        scatterplot.transform.rotation = scatterplotTransform.rotation;
        scatterplot.SetAsPrototype();

        foreach (Menu menu in scatterplotButtons)
        {
            menu.targetChart = scatterplot;
        }

        // Configure scatterplot matrix
        scatterplotMatrix = ChartManager.Instance.CreateVisualisation("WorkscreenScatterplotMatrix");
        scatterplotMatrix.VisualisationType = AbstractVisualisation.VisualisationTypes.SCATTERPLOT_MATRIX;
        scatterplotMatrix.GeometryType = AbstractVisualisation.GeometryType.Points;
        scatterplotMatrix.transform.position = scatterplotMatrixTransform.position;
        scatterplotMatrix.transform.rotation = scatterplotMatrixTransform.rotation;
        scatterplotMatrix.Width = 0.9f;
        scatterplotMatrix.Height = 0.9f;
        scatterplotMatrix.Depth = 0.9f;

        // Configure facets
        // TODO
        facet = ChartManager.Instance.CreateVisualisation("WorkscreenFacet");
        facet.SetAsPrototype();

        ShowScatterplotMatrix();
    }

    public void ShowScatterplot()
    {
        ToggleState(true, false, false);
    }

    public void ShowScatterplotMatrix()
    {
        ToggleState(false, true, false);
    }

    public void ShowFacet()
    {
        ToggleState(false, false, true);
    }

    private void ToggleState(bool sp, bool spm, bool f)
    {
        scatterplot.gameObject.SetActive(sp);
        scatterplotMatrix.gameObject.SetActive(spm);
        facet.gameObject.SetActive(f);

        foreach (Menu button in scatterplotButtons)
        {
            button.gameObject.SetActive(sp);
        }

        foreach (GameObject button in scatterplotMatrixButtons)
        {
            button.SetActive(spm);
        }

        foreach (GameObject button in facetButtons)
        {
            button.SetActive(f);
        }
    }

    public void SizeSliderValueChanged(float value)
    {
        scatterplot.Size = value;
        scatterplotMatrix.Size = value;
        facet.Size = value;
    }

    public void RedSliderValueChanged(float value)
    {
        Color color = scatterplot.Color;
        color.r = value;

        scatterplot.Color = color;
        scatterplotMatrix.Color = color;
        facet.Color = color;
    }

    public void GreenSliderValueChanged(float value)
    {
        Color color = scatterplot.Color;
        color.g = value;

        scatterplot.Color = color;
        scatterplotMatrix.Color = color;
        facet.Color = color;
    }

    public void BlueSliderValueChanged(float value)
    {
        Color color = scatterplot.Color;
        color.b = value;

        scatterplot.Color = color;
        scatterplotMatrix.Color = color;
        facet.Color = color;
    }

    public void ScatterplotMatrixSizeSlider(float value)
    {
        scatterplotMatrix.ScatterplotMatrixSize = (int)value;
    }
}
