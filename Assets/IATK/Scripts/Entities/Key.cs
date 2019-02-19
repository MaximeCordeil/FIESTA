﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace IATK
{
    public class Key : MonoBehaviour
    {

        public LineRenderer gradientColorLineRenderer;
        public TextMeshPro Legend;

        private string legend="";
        
        public void UpdateProperties(AbstractVisualisation.PropertyType propertyType, Visualisation v)
        {
            legend = "";

            // Facet by
            if (v.attributeFilters != null && v.attributeFilters.Length > 0 &&
                v.attributeFilters[0].Attribute != "Undefined")
            {
                legend += "<b>Facet By:</b> " + v.attributeFilters[0].Attribute + "\n";
            }

            // Size by
            if (v.sizeDimension != "Undefined")
                legend += "<b>Size By:</b> " + v.sizeDimension + "\n";

            // Linking dimension
            if (v.linkingDimension != "Undefined")
                legend += "<b>Linking Attribute:<b> " + v.linkingDimension + "\n";

            // Visualisation can only have a color dimension or a color palette dimension, not both
            // Color by
            if (v.colourDimension != "Undefined")
            {
                gradientColorLineRenderer.gameObject.SetActive(true);
                legend += "<b>Colour By:</b> " + v.colourDimension + "\n";
                SetGradientColor(v.dimensionColour);
            }
            // Color palette
            else if (v.colorPaletteDimension != "Undefined")
            {
                gradientColorLineRenderer.gameObject.SetActive(false);
                legend += "<b>Colour:</b> " + v.colorPaletteDimension + "\n";
                float[] uniqueValues = v.dataSource[v.colorPaletteDimension].MetaData.categories;
                string[] stringValues = new string[uniqueValues.Length];

                for (int i = 0; i < uniqueValues.Length; i++)
                    stringValues[i] = v.dataSource.getOriginalValue(uniqueValues[i], v.colorPaletteDimension).ToString();

                for (int i = 0; i < v.coloursPalette.Length; i++)
                {
                    legend += "<color=#" + ColorUtility.ToHtmlStringRGB(v.coloursPalette[i]) + "> *** </color>" + stringValues[i] + "\n";
                }
            }

            // Hide the gradient if no color by
            if (v.colourDimension == "Undefined")
            {
                gradientColorLineRenderer.gameObject.SetActive(false);
            }

            Legend.text = legend;
        }

        /// <summary>
        /// Binds a Gradient color object to the line renderer
        /// </summary>
        /// <param name="gradient"></param>
        void SetGradientColor(Gradient gradient)
        {
            Vector3[] vertices = new Vector3[gradient.colorKeys.Length];
            GradientColorKey[] colorKeys = gradient.colorKeys;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(colorKeys[i].time, 0f, 0f);

            gradientColorLineRenderer.positionCount = vertices.Length;
            gradientColorLineRenderer.SetPositions(vertices);
            gradientColorLineRenderer.colorGradient = gradient;
        }

    }
}