using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using NationalInstruments.Vision.WindowsForms;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Analysis;
using Vision_Assistant.Utilities;
using desay.ProductData;

namespace Vision_Assistant
{
    static class LeftOut_Processing
    {
        public static Collection<PatternMatchReport> pmResults;
        public static ParticleMeasurementsReport vaParticleReport;
        public static ParticleMeasurementsReport vaParticleReportCalibrated;
        public static double[] distances;
        public static PointContour[] contours;


        private static Collection<GeometricEdgeBasedPatternMatch> IVA_MatchGeometricPattern2(VisionImage image,
                                                                                            string templatePath,
                                                                                            CurveOptions curveOptions,
                                                                                            MatchGeometricPatternEdgeBasedOptions matchOptions,
                                                                                            IVA_Data ivaData,
                                                                                            int stepIndex,
                                                                                            Roi roi)
        {

            // Geometric Matching (Edge Based)

            // Creates the image template.
            using (VisionImage imageTemplate = new VisionImage(ImageType.U8, 7))
            {
                // Read the image template.
                imageTemplate.ReadVisionFile(templatePath);

                Collection<GeometricEdgeBasedPatternMatch> gpmResults = Algorithms.MatchGeometricPatternEdgeBased(image, imageTemplate, curveOptions, matchOptions, roi);

                // Store the results in the data structure.

                // First, delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, stepIndex);

                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("# Matches", gpmResults.Count));

                for (int i = 0; i < gpmResults.Count; ++i)
                {
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.X Position (Pix.)", i + 1), gpmResults[i].Position.X));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Y Position (Pix.)", i + 1), gpmResults[i].Position.Y));

                    // If the image is calibrated, log the calibrated results.
                    if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                    {
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.X Position (World)", i + 1), gpmResults[i].CalibratedPosition.X));
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Y Position (World)", i + 1), gpmResults[i].CalibratedPosition.Y));
                    }

                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Angle (degrees)", i + 1), gpmResults[i].Rotation));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Scale", i + 1), gpmResults[i].Scale));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Score", i + 1), gpmResults[i].Score));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Occlusion", i + 1), gpmResults[i].Occlusion));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Template Target Curve Score", i + 1), gpmResults[i].TemplateMatchCurveScore));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Correlation Score", i + 1), gpmResults[i].CorrelationScore));
                }

                return gpmResults;
            }
        }

        private static void IVA_CoordSys(int coordSysIndex,
                                         int originStepIndex,
                                         int originResultIndex,
                                         int angleStepIndex,
                                         int angleResultIndex,
                                         double baseOriginX,
                                         double baseOriginY,
                                         double baseAngle,
                                         AxisOrientation baseAxisOrientation,
                                         int mode,
                                         IVA_Data ivaData)
        {

            ivaData.baseCoordinateSystems[coordSysIndex].Origin.X = baseOriginX;
            ivaData.baseCoordinateSystems[coordSysIndex].Origin.Y = baseOriginY;
            ivaData.baseCoordinateSystems[coordSysIndex].Angle = baseAngle;
            ivaData.baseCoordinateSystems[coordSysIndex].AxisOrientation = baseAxisOrientation;

            ivaData.MeasurementSystems[coordSysIndex].Origin.X = baseOriginX;
            ivaData.MeasurementSystems[coordSysIndex].Origin.Y = baseOriginY;
            ivaData.MeasurementSystems[coordSysIndex].Angle = baseAngle;
            ivaData.MeasurementSystems[coordSysIndex].AxisOrientation = baseAxisOrientation;

            switch (mode)
            {
                // Horizontal motion
                case 0:
                    ivaData.MeasurementSystems[coordSysIndex].Origin.X = Functions.IVA_GetNumericResult(ivaData, originStepIndex, originResultIndex);
                    break;
                // Vertical motion
                case 1:
                    ivaData.MeasurementSystems[coordSysIndex].Origin.Y = Functions.IVA_GetNumericResult(ivaData, originStepIndex, originResultIndex + 1);
                    break;
                // Horizontal and vertical motion
                case 2:
                    ivaData.MeasurementSystems[coordSysIndex].Origin = Functions.IVA_GetPoint(ivaData, originStepIndex, originResultIndex);
                    break;
                // Horizontal, vertical and angular motion
                case 3:
                    ivaData.MeasurementSystems[coordSysIndex].Origin = Functions.IVA_GetPoint(ivaData, originStepIndex, originResultIndex);
                    ivaData.MeasurementSystems[coordSysIndex].Angle = Functions.IVA_GetNumericResult(ivaData, angleStepIndex, angleResultIndex);
                    break;
            }
        }

        private static ExtractContourReport IVA_ExtractContour(VisionImage image,
                                                               Roi roi,
                                                               ExtractContourDirection direction,
                                                               CurveParameters curveSettings,
                                                               ConnectionConstraintType[] constraintTypeArray,
                                                               double[] constraintMinArray,
                                                               double[] constraintMaxArray,
                                                               ExtractContourSelection selection)
        {

            // Build the ConnectionConstraint Collection
            Collection<ConnectionConstraint> constraints = new Collection<ConnectionConstraint>();
            for (int i = 0; i < constraintTypeArray.Length; ++i)
            {
                constraints.Add(new ConnectionConstraint(constraintTypeArray[i], new Range(constraintMinArray[i], constraintMaxArray[i])));
            }
            // Extract contours from image
            return Algorithms.ExtractContour(image, roi, direction, curveSettings, constraints, selection);
        }

        private static void IVA_Particle(VisionImage image,
                                Connectivity connectivity,
                                Collection<MeasurementType> pPixelMeasurements,
                                Collection<MeasurementType> pCalibratedMeasurements,
                                IVA_Data ivaData,
                                int stepIndex,
                                out ParticleMeasurementsReport partReport,
                                out ParticleMeasurementsReport partReportCal)
        {

            // Computes the requested pixel measurements.
            if (pPixelMeasurements.Count != 0)
            {
                partReport = Algorithms.ParticleMeasurements(image, pPixelMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Pixel);
            }
            else
            {
                partReport = new ParticleMeasurementsReport();
            }

            // Computes the requested calibrated measurements.
            if (pCalibratedMeasurements.Count != 0)
            {
                partReportCal = Algorithms.ParticleMeasurements(image, pCalibratedMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Calibrated);
            }
            else
            {
                partReportCal = new ParticleMeasurementsReport();
            }

            // Computes the center of mass of each particle to log as results.
            ParticleMeasurementsReport centerOfMass;
            Collection<MeasurementType> centerOfMassMeasurements = new Collection<MeasurementType>();
            centerOfMassMeasurements.Add(MeasurementType.CenterOfMassX);
            centerOfMassMeasurements.Add(MeasurementType.CenterOfMassY);

            if ((image.InfoTypes & InfoTypes.Calibration) != 0)
            {
                centerOfMass = Algorithms.ParticleMeasurements(image, centerOfMassMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Both);
            }
            else
            {
                centerOfMass = Algorithms.ParticleMeasurements(image, centerOfMassMeasurements, connectivity, ParticleMeasurementsCalibrationMode.Pixel);
            }

            // Delete all the results of this step (from a previous iteration)
            Functions.IVA_DisposeStepResults(ivaData, stepIndex);

            ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Object #", centerOfMass.PixelMeasurements.GetLength(0)));

            if (centerOfMass.PixelMeasurements.GetLength(0) > 0)
            {
                for (int i = 0; i < centerOfMass.PixelMeasurements.GetLength(0); ++i)
                {
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.X Position (Pix.)", i + 1), centerOfMass.PixelMeasurements[i, 0]));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.Y Position (Pix.)", i + 1), centerOfMass.PixelMeasurements[i, 1]));

                    // If the image is calibrated, also store the real world coordinates.
                    if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                    {
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.X Position (Calibrated)", i + 1), centerOfMass.CalibratedMeasurements[i, 0]));
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Particle {0}.Y Position (Calibrated)", i + 1), centerOfMass.CalibratedMeasurements[i, 1]));
                    }
                }
            }
        }

        private static Collection<PatternMatchReport> IVA_MatchPattern(VisionImage image,
                                                         IVA_Data ivaData,
                                                         string templatePath,
                                                         MatchingAlgorithm algorithm,
                                                         float[] angleRangeMin,
                                                         float[] angleRangeMax,
                                                         int[] advOptionsItems,
                                                         double[] advOptionsValues,
                                                         int numAdvancedOptions,
                                                         int matchesRequested,
                                                         float score,
                                                         Roi roi,
                                                         int stepIndex)
        {

            FileInformation fileInfo;
            fileInfo = Algorithms.GetFileInformation(templatePath);
            using (VisionImage imageTemplate = new VisionImage(fileInfo.ImageType, 7))
            {
                int numObjectResults = 4;
                Collection<PatternMatchReport> patternMatchingResults = new Collection<PatternMatchReport>();

                // Read the image template.
                imageTemplate.ReadVisionFile(templatePath);

                // If the image is calibrated, we also need to log the calibrated position (x and y) and angle -> 7 results instead of 4
                if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                {
                    numObjectResults = 7;
                }

                // Set the angle range.
                Collection<RotationAngleRange> angleRange = new Collection<RotationAngleRange>();
                for (int i = 0; i < 2; ++i)
                {
                    angleRange.Add(new RotationAngleRange(angleRangeMin[i], angleRangeMax[i]));
                }

                // Set the advanced options.
                Collection<PMMatchAdvancedSetupDataOption> advancedMatchOptions = new Collection<PMMatchAdvancedSetupDataOption>();
                for (int i = 0; i < numAdvancedOptions; ++i)
                {
                    advancedMatchOptions.Add(new PMMatchAdvancedSetupDataOption((MatchSetupOption)advOptionsItems[i], advOptionsValues[i]));
                }

                // Searches for areas in the image that match a given pattern.
                patternMatchingResults = Algorithms.MatchPattern3(image, imageTemplate, algorithm, matchesRequested, score, angleRange, roi, advancedMatchOptions);

                // ////////////////////////////////////////
                // Store the results in the data structure.
                // ////////////////////////////////////////

                // First, delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, stepIndex);

                if (patternMatchingResults.Count > 0)
                {
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result("# of objects", patternMatchingResults.Count));

                    for (int i = 0; i < patternMatchingResults.Count; ++i)
                    {
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.X Position (Pix.)", i + 1), patternMatchingResults[i].Position.X));
                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Y Position (Pix.)", i + 1), patternMatchingResults[i].Position.Y));

                        // If the image is calibrated, add the calibrated positions.
                        if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                        {
                            ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.X Position (World)", i + 1), patternMatchingResults[i].CalibratedPosition.X));
                            ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Y Position (World)", i + 1), patternMatchingResults[i].CalibratedPosition.Y));
                        }

                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Angle (degrees)", i + 1), patternMatchingResults[i].Rotation));
                        if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                        {
                            ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Calibrated Angle (degrees)", i + 1), patternMatchingResults[i].CalibratedRotation));
                        }

                        ivaData.stepResults[stepIndex].results.Add(new IVA_Result(String.Format("Match {0}.Score", i + 1), patternMatchingResults[i].Score));
                    }
                }

                return patternMatchingResults;

            }
        }


        public static PaletteType ProcessImage(VisionImage image)
        {
            contours = new PointContour[14];
            // Initialize the IVA_Data structure to pass results and coordinate systems.
            IVA_Data ivaData = new IVA_Data(22, 1);

            // Extract Color Plane
            using (VisionImage plane = new VisionImage(ImageType.U8, 7))
            {
                // Extract the red color plane and copy it to the main image.
                Algorithms.ExtractColorPlanes(image, ColorMode.Rgb, plane, null, null);
                Algorithms.Copy(plane, image);
            }

            // Creates a new, empty region of interest.
            Roi roi = new Roi();
            // Creates a new RectangleContour using the given values.
            RectangleContour vaRect = new RectangleContour(645, 714, 1069, 444);
            roi.Add(vaRect);
            // MatchPattern Grayscale
            string dicpath = System.Windows.Forms.Application.StartupPath;
            string vaTemplateFile = dicpath + $"{ @"/ImageConfig/LeftSpeciPos.png"}";
            MatchingAlgorithm matchAlgorithm = MatchingAlgorithm.MatchGrayValuePyramid;
            float[] minAngleVals = { -10, 0 };
            float[] maxAngleVals = { 10, 0 };
            int[] advancedOptionsItems = { 100, 102, 106, 107, 108, 109, 114, 116, 117, 118, 111, 112, 113, 103, 104, 105 };
            double[] advancedOptionsValues = { 5, 10, 300, 0, 6, 1, 25, 0, 0, 0, 20, 10, 20, 1, 20, 0 };
            int numberAdvOptions = 16;
            int vaNumMatchesRequested = 1;
            float vaMinMatchScore = 700;
            pmResults = IVA_MatchPattern(image, ivaData, vaTemplateFile, matchAlgorithm, minAngleVals, maxAngleVals, advancedOptionsItems, advancedOptionsValues, numberAdvOptions, vaNumMatchesRequested, vaMinMatchScore, roi, 2);
            roi.Dispose();

            if (pmResults.Count == 1)
            {

                // Set Coordinate System
                int vaCoordSystemIndex = 0;
                int stepIndexOrigin = 2;
                int resultIndexOrigin = 1;
                int stepIndexAngle = 2;
                int resultIndexAngle = 3;
                double refSysOriginX = 1348.169;
                double refSysOriginY = 874.5308;
                double refSysAngle = 0.3424073;
                AxisOrientation refSysAxisOrientation = AxisOrientation.Direct;
                int vaCoordSystemType = 3;
                IVA_CoordSys(vaCoordSystemIndex, stepIndexOrigin, resultIndexOrigin, stepIndexAngle, resultIndexAngle, refSysOriginX, refSysOriginY, refSysAngle, refSysAxisOrientation, vaCoordSystemType, ivaData);

                // Manual Threshold
                Algorithms.Threshold(image, image, new Range(Position.Instance.Threshold_Left, 255), true, 1);

                // Advanced Morphology: Remove Border Objects - Eliminates particles touching the border of the image.
                Algorithms.RejectBorder(image, image, Connectivity.Connectivity8);

                // Basic Morphology - Applies morphological transformations to binary images.
                int[] vaCoefficients = { 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0 };
                StructuringElement vaStructElem = new StructuringElement(5, 5, vaCoefficients);
                vaStructElem.Shape = StructuringElementShape.Square;
                // Applies morphological transformations
                Algorithms.Morphology(image, image, MorphologyMethod.Open, vaStructElem);

                // Advanced Morphology: Remove Objects
                int[] vaCoefficients2 = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                StructuringElement vaStructElem2 = new StructuringElement(3, 3, vaCoefficients2);
                vaStructElem.Shape = StructuringElementShape.Square;
                // Filters particles based on their size.
                Algorithms.RemoveParticle(image, image, 10, SizeToKeep.KeepLarge, Connectivity.Connectivity8, vaStructElem2);

                VisionImage newimage = new VisionImage(ImageType.U8, 7);
                Algorithms.FillHoles(image, newimage, Connectivity.Connectivity8);
                Collection<MeasurementType> vaPixelMeasurements = new Collection<MeasurementType>(new MeasurementType[] { MeasurementType.Area });
                Collection<MeasurementType> vaCalibratedMeasurements = new Collection<MeasurementType>(new MeasurementType[] { });
                IVA_Particle(newimage, Connectivity.Connectivity8, vaPixelMeasurements, vaCalibratedMeasurements, ivaData, 16, out vaParticleReport, out vaParticleReportCalibrated);

                double[,] area = vaParticleReport.PixelMeasurements;
                double Maxarea = 0;
                for (int i = 0; i < area.GetLength(0); i++)
                {
                    for (int j = 0; j < area.GetLength(1); j++)
                    {
                        if (area[i, j] > Maxarea)
                        {
                            Maxarea = area[i, j];
                        }
                    }
                }
                newimage.Dispose();

                if (Maxarea < 1000000)
                {
                    distances = new double[] { 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99 };
                    for (int i = 0; i < 14; i++)
                    {
                        contours[i] = new PointContour(0, 0);
                    }
                    ivaData.Dispose();
                    return PaletteType.Binary;
                }

                double MaxDis1 = 0, MaxDis2 = 0, MaxDis3 = 0, MaxDis4 = 0, MaxDis5 = 0, MaxDis6 = 0, MaxDis7 = 0, MaxDis8 = 0, MaxDis9 = 0, MaxDis10 = 0, MaxDis11 = 0, MaxDis12 = 0, MaxDis13 = 0, MaxDis14 = 0;
                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi2 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter = new PointContour(1457, 206);
                    AnnulusContour vaOval = new AnnulusContour(vaCenter, 7, 97, 0.341, 89.606);
                    roi2.Add(vaOval);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex = 0;
                    Algorithms.TransformRoi(roi2, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex], ivaData.MeasurementSystems[coordSystemIndex]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray = { };
                    double[] vaConstraintMaxArray = { };
                    ConnectionConstraintType[] vaConstraintTypeArray = { };
                    ExtractContourReport vaExtractReport = IVA_ExtractContour(image, roi2, ExtractContourDirection.AnnulusOuterInner, vaCurveParams, vaConstraintTypeArray, vaConstraintMinArray, vaConstraintMaxArray, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay, vaEquationOverlay);
                    ComputeDistanceReport vaDistanceReport = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport.Distances.Count; i++)
                    {
                        if (vaDistanceReport.Distances[i].Distance > MaxDis1)
                        {
                            MaxDis1 = vaDistanceReport.Distances[i].Distance;

                            contours[0] = vaDistanceReport.Distances[i].CurrentPoint;
                        }
                    }
                    roi2.Dispose();
                }
                catch
                {
                    MaxDis1 = 0;

                    contours[0] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi3 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter2 = new PointContour(1492.5, 263);
                    RotatedRectangleContour vaRotatedRect = new RotatedRectangleContour(vaCenter2, 101, 110, 0.341);
                    roi3.Add(vaRotatedRect);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex2 = 0;
                    Algorithms.TransformRoi(roi3, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex2], ivaData.MeasurementSystems[coordSystemIndex2]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams2 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray2 = { };
                    double[] vaConstraintMaxArray2 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray2 = { };
                    ExtractContourReport vaExtractReport2 = IVA_ExtractContour(image, roi3, ExtractContourDirection.RectRightLeft, vaCurveParams2, vaConstraintTypeArray2, vaConstraintMinArray2, vaConstraintMaxArray2, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay2 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay2 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport = Algorithms.ContourFitLine(image, 4);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay2, vaEquationOverlay2);
                    ComputeDistanceReport vaDistanceReport2 = Algorithms.ContourComputeDistances(image, image, 0);


                    for (int i = 0; i < vaDistanceReport2.Distances.Count; i++)
                    {
                        if (vaDistanceReport2.Distances[i].Distance > MaxDis2)
                        {
                            MaxDis2 = vaDistanceReport2.Distances[i].Distance;

                            contours[1] = vaDistanceReport2.Distances[i].CurrentPoint;
                        }
                    }

                    roi3.Dispose();
                }
                catch
                {
                    MaxDis2 = 0;

                    contours[1] = new PointContour(0, 0);
                }

                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi4 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter3 = new PointContour(1401, 319);
                    AnnulusContour vaOval2 = new AnnulusContour(vaCenter3, 40, 143, 297.606, 359.594);
                    roi4.Add(vaOval2);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex3 = 0;
                    Algorithms.TransformRoi(roi4, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex3], ivaData.MeasurementSystems[coordSystemIndex3]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams3 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray3 = { };
                    double[] vaConstraintMaxArray3 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray3 = { };
                    ExtractContourReport vaExtractReport3 = IVA_ExtractContour(image, roi4, ExtractContourDirection.AnnulusOuterInner, vaCurveParams3, vaConstraintTypeArray3, vaConstraintMinArray3, vaConstraintMaxArray3, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay3 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay3 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport2 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay3, vaEquationOverlay3);
                    ComputeDistanceReport vaDistanceReport3 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport3.Distances.Count; i++)
                    {
                        if (vaDistanceReport3.Distances[i].Distance > MaxDis3)
                        {
                            MaxDis3 = vaDistanceReport3.Distances[i].Distance;

                            contours[2] = vaDistanceReport3.Distances[i].CurrentPoint;
                        }
                    }

                    roi4.Dispose();
                }
                catch
                {
                    MaxDis3 = 0;

                    contours[2] = new PointContour(0, 0);
                }

                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi5 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter4 = new PointContour(2050, 1220.5);
                    RotatedRectangleContour vaRotatedRect2 = new RotatedRectangleContour(vaCenter4, 100, 101, 0.341);
                    roi5.Add(vaRotatedRect2);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex4 = 0;
                    Algorithms.TransformRoi(roi5, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex4], ivaData.MeasurementSystems[coordSystemIndex4]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams4 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray4 = { };
                    double[] vaConstraintMaxArray4 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray4 = { };
                    ExtractContourReport vaExtractReport4 = IVA_ExtractContour(image, roi5, ExtractContourDirection.RectRightLeft, vaCurveParams4, vaConstraintTypeArray4, vaConstraintMinArray4, vaConstraintMaxArray4, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay4 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay4 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport2 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay4, vaEquationOverlay4);
                    ComputeDistanceReport vaDistanceReport4 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport4.Distances.Count; i++)
                    {
                        if (vaDistanceReport4.Distances[i].Distance > MaxDis4)
                        {
                            MaxDis4 = vaDistanceReport4.Distances[i].Distance;

                            contours[3] = vaDistanceReport4.Distances[i].CurrentPoint;
                        }
                    }
                    roi5.Dispose();
                }
                catch
                {
                    MaxDis4 = 0;

                    contours[3] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi6 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter5 = new PointContour(1482, 477);
                    AnnulusContour vaOval3 = new AnnulusContour(vaCenter5, 19, 131, 120.425, 240.415);
                    roi6.Add(vaOval3);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex5 = 0;
                    Algorithms.TransformRoi(roi6, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex5], ivaData.MeasurementSystems[coordSystemIndex5]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams5 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray5 = { };
                    double[] vaConstraintMaxArray5 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray5 = { };
                    ExtractContourReport vaExtractReport5 = IVA_ExtractContour(image, roi6, ExtractContourDirection.AnnulusInnerOuter, vaCurveParams5, vaConstraintTypeArray5, vaConstraintMinArray5, vaConstraintMaxArray5, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay5 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay5 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport3 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay5, vaEquationOverlay5);
                    ComputeDistanceReport vaDistanceReport5 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport5.Distances.Count; i++)
                    {
                        if (vaDistanceReport5.Distances[i].Distance > MaxDis5)
                        {
                            MaxDis5 = vaDistanceReport5.Distances[i].Distance;

                            contours[4] = vaDistanceReport5.Distances[i].CurrentPoint;
                        }
                    }
                    roi6.Dispose();
                }
                catch
                {
                    MaxDis5 = 0;

                    contours[4] = new PointContour(0, 0);
                }

                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi7 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter6 = new PointContour(2149, 1220);
                    RotatedRectangleContour vaRotatedRect3 = new RotatedRectangleContour(vaCenter6, 100, 100, 0.342);
                    roi7.Add(vaRotatedRect3);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex6 = 0;
                    Algorithms.TransformRoi(roi7, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex6], ivaData.MeasurementSystems[coordSystemIndex6]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams6 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray6 = { };
                    double[] vaConstraintMaxArray6 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray6 = { };
                    ExtractContourReport vaExtractReport6 = IVA_ExtractContour(image, roi7, ExtractContourDirection.RectRightLeft, vaCurveParams6, vaConstraintTypeArray6, vaConstraintMinArray6, vaConstraintMaxArray6, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay6 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay6 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport3 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay6, vaEquationOverlay6);
                    ComputeDistanceReport vaDistanceReport6 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport6.Distances.Count; i++)
                    {
                        if (vaDistanceReport6.Distances[i].Distance > MaxDis6)
                        {
                            MaxDis6 = vaDistanceReport6.Distances[i].Distance;

                            contours[5] = vaDistanceReport6.Distances[i].CurrentPoint;
                        }
                    }
                    roi7.Dispose();
                }
                catch
                {
                    MaxDis6 = 0;

                    contours[5] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi8 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter7 = new PointContour(1405, 621);
                    AnnulusContour vaOval4 = new AnnulusContour(vaCenter7, 36, 126, 0.251, 61.196);
                    roi8.Add(vaOval4);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex7 = 0;
                    Algorithms.TransformRoi(roi8, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex7], ivaData.MeasurementSystems[coordSystemIndex7]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams7 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray7 = { };
                    double[] vaConstraintMaxArray7 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray7 = { };
                    ExtractContourReport vaExtractReport7 = IVA_ExtractContour(image, roi8, ExtractContourDirection.AnnulusOuterInner, vaCurveParams7, vaConstraintTypeArray7, vaConstraintMinArray7, vaConstraintMaxArray7, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay7 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay7 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport4 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay7, vaEquationOverlay7);
                    ComputeDistanceReport vaDistanceReport7 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport7.Distances.Count; i++)
                    {
                        if (vaDistanceReport7.Distances[i].Distance > MaxDis7)
                        {
                            MaxDis7 = vaDistanceReport7.Distances[i].Distance;

                            contours[6] = vaDistanceReport7.Distances[i].CurrentPoint;
                        }
                    }
                    roi8.Dispose();
                }
                catch
                {
                    MaxDis7 = 0;

                    contours[6] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi9 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter8 = new PointContour(1492, 1206);
                    RotatedRectangleContour vaRotatedRect4 = new RotatedRectangleContour(vaCenter8, 102, 1158, 0.341);
                    roi9.Add(vaRotatedRect4);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex8 = 0;
                    Algorithms.TransformRoi(roi9, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex8], ivaData.MeasurementSystems[coordSystemIndex8]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams8 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray8 = { };
                    double[] vaConstraintMaxArray8 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray8 = { };
                    ExtractContourReport vaExtractReport8 = IVA_ExtractContour(image, roi9, ExtractContourDirection.RectRightLeft, vaCurveParams8, vaConstraintTypeArray8, vaConstraintMinArray8, vaConstraintMaxArray8, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay8 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay8 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport4 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay8, vaEquationOverlay8);
                    ComputeDistanceReport vaDistanceReport8 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport8.Distances.Count; i++)
                    {
                        if (vaDistanceReport8.Distances[i].Distance > MaxDis8)
                        {
                            MaxDis8 = vaDistanceReport8.Distances[i].Distance;

                            contours[7] = vaDistanceReport8.Distances[i].CurrentPoint;
                        }
                    }
                    roi9.Dispose();
                }
                catch
                {
                    MaxDis8 = 0;

                    contours[7] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi10 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter9 = new PointContour(1439, 1786);
                    AnnulusContour vaOval5 = new AnnulusContour(vaCenter9, 11, 93, 270.251, 0.251);
                    roi10.Add(vaOval5);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex9 = 0;
                    Algorithms.TransformRoi(roi10, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex9], ivaData.MeasurementSystems[coordSystemIndex9]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams9 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray9 = { };
                    double[] vaConstraintMaxArray9 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray9 = { };
                    ExtractContourReport vaExtractReport9 = IVA_ExtractContour(image, roi10, ExtractContourDirection.AnnulusOuterInner, vaCurveParams9, vaConstraintTypeArray9, vaConstraintMinArray9, vaConstraintMaxArray9, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay9 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay9 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport5 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay9, vaEquationOverlay9);
                    ComputeDistanceReport vaDistanceReport9 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport9.Distances.Count; i++)
                    {
                        if (vaDistanceReport9.Distances[i].Distance > MaxDis9)
                        {
                            MaxDis9 = vaDistanceReport9.Distances[i].Distance;

                            contours[8] = vaDistanceReport9.Distances[i].CurrentPoint;
                        }
                    }
                    roi10.Dispose();
                }
                catch
                {
                    MaxDis9 = 0;

                    contours[8] = new PointContour(0, 0);
                }

                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi11 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter10 = new PointContour(1167, 1844.5);
                    RotatedRectangleContour vaRotatedRect5 = new RotatedRectangleContour(vaCenter10, 538, 73, 0.251);
                    roi11.Add(vaRotatedRect5);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex10 = 0;
                    Algorithms.TransformRoi(roi11, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex10], ivaData.MeasurementSystems[coordSystemIndex10]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams10 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray10 = { };
                    double[] vaConstraintMaxArray10 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray10 = { };
                    ExtractContourReport vaExtractReport10 = IVA_ExtractContour(image, roi11, ExtractContourDirection.RectBottomTop, vaCurveParams10, vaConstraintTypeArray10, vaConstraintMinArray10, vaConstraintMaxArray10, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay10 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay10 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport5 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay10, vaEquationOverlay10);
                    ComputeDistanceReport vaDistanceReport10 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport10.Distances.Count; i++)
                    {
                        if (vaDistanceReport10.Distances[i].Distance > MaxDis10)
                        {
                            MaxDis10 = vaDistanceReport10.Distances[i].Distance;

                            contours[9] = vaDistanceReport10.Distances[i].CurrentPoint;
                        }
                    }
                    roi11.Dispose();
                }
                catch
                {
                    MaxDis10 = 0;

                    contours[9] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi12 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter11 = new PointContour(896, 1788);
                    AnnulusContour vaOval6 = new AnnulusContour(vaCenter11, 9, 95, 180.251, 270.251);
                    roi12.Add(vaOval6);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex11 = 0;
                    Algorithms.TransformRoi(roi12, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex11], ivaData.MeasurementSystems[coordSystemIndex11]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams11 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray11 = { };
                    double[] vaConstraintMaxArray11 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray11 = { };
                    ExtractContourReport vaExtractReport11 = IVA_ExtractContour(image, roi12, ExtractContourDirection.AnnulusOuterInner, vaCurveParams11, vaConstraintTypeArray11, vaConstraintMinArray11, vaConstraintMaxArray11, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay11 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay11 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport6 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay11, vaEquationOverlay11);
                    ComputeDistanceReport vaDistanceReport11 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport11.Distances.Count; i++)
                    {
                        if (vaDistanceReport11.Distances[i].Distance > MaxDis11)
                        {
                            MaxDis11 = vaDistanceReport11.Distances[i].Distance;

                            contours[10] = vaDistanceReport11.Distances[i].CurrentPoint;
                        }
                    }
                    roi12.Dispose();
                }
                catch
                {
                    MaxDis11 = 0;

                    contours[10] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi13 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter12 = new PointContour(849, 1000.5);
                    RotatedRectangleContour vaRotatedRect6 = new RotatedRectangleContour(vaCenter12, 100, 1571, 0.25);
                    roi13.Add(vaRotatedRect6);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex12 = 0;
                    Algorithms.TransformRoi(roi13, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex12], ivaData.MeasurementSystems[coordSystemIndex12]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams12 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray12 = { };
                    double[] vaConstraintMaxArray12 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray12 = { };
                    ExtractContourReport vaExtractReport12 = IVA_ExtractContour(image, roi13, ExtractContourDirection.RectLeftRight, vaCurveParams12, vaConstraintTypeArray12, vaConstraintMinArray12, vaConstraintMaxArray12, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay12 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay12 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport6 = Algorithms.ContourFitLine(image, 7);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay12, vaEquationOverlay12);
                    ComputeDistanceReport vaDistanceReport12 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport12.Distances.Count; i++)
                    {
                        if (vaDistanceReport12.Distances[i].Distance > MaxDis12)
                        {
                            MaxDis12 = vaDistanceReport12.Distances[i].Distance;

                            contours[11] = vaDistanceReport12.Distances[i].CurrentPoint;
                        }
                    }
                    roi13.Dispose();
                }
                catch
                {
                    MaxDis12 = 0;

                    contours[11] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi14 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter13 = new PointContour(903, 213);
                    AnnulusContour vaOval7 = new AnnulusContour(vaCenter13, 12, 104, 90.34, 180.018);
                    roi14.Add(vaOval7);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex13 = 0;
                    Algorithms.TransformRoi(roi14, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex13], ivaData.MeasurementSystems[coordSystemIndex13]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams13 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray13 = { };
                    double[] vaConstraintMaxArray13 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray13 = { };
                    ExtractContourReport vaExtractReport13 = IVA_ExtractContour(image, roi14, ExtractContourDirection.AnnulusOuterInner, vaCurveParams13, vaConstraintTypeArray13, vaConstraintMinArray13, vaConstraintMaxArray13, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay13 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay13 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport7 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay13, vaEquationOverlay13);
                    ComputeDistanceReport vaDistanceReport13 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport13.Distances.Count; i++)
                    {
                        if (vaDistanceReport13.Distances[i].Distance > MaxDis13)
                        {
                            MaxDis13 = vaDistanceReport13.Distances[i].Distance;

                            contours[12] = vaDistanceReport13.Distances[i].CurrentPoint;
                        }
                    }
                    roi14.Dispose();
                }
                catch
                {
                    MaxDis13 = 0;

                    contours[12] = new PointContour(0, 0);

                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi15 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter14 = new PointContour(1179.5, 158.5);
                    RotatedRectangleContour vaRotatedRect7 = new RotatedRectangleContour(vaCenter14, 547, 87, 0.251);
                    roi15.Add(vaRotatedRect7);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex14 = 0;
                    Algorithms.TransformRoi(roi15, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex14], ivaData.MeasurementSystems[coordSystemIndex14]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams14 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray14 = { };
                    double[] vaConstraintMaxArray14 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray14 = { };
                    ExtractContourReport vaExtractReport14 = IVA_ExtractContour(image, roi15, ExtractContourDirection.RectTopBottom, vaCurveParams14, vaConstraintTypeArray14, vaConstraintMinArray14, vaConstraintMaxArray14, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay14 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay14 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport7 = Algorithms.ContourFitLine(image, 7);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay14, vaEquationOverlay14);
                    ComputeDistanceReport vaDistanceReport14 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport14.Distances.Count; i++)
                    {
                        if (vaDistanceReport14.Distances[i].Distance > MaxDis14)
                        {
                            MaxDis14 = vaDistanceReport14.Distances[i].Distance;

                            contours[13] = vaDistanceReport14.Distances[i].CurrentPoint;
                        }
                    }
                    roi15.Dispose();
                }
                catch
                {
                    MaxDis14 = 0;

                    contours[13] = new PointContour(0, 0);
                }
                distances = new double[] { MaxDis1, MaxDis2, MaxDis3, MaxDis4, MaxDis5, MaxDis6, MaxDis7, MaxDis8, MaxDis9, MaxDis10, MaxDis11, MaxDis12, MaxDis13, MaxDis14 };

            }
            else
            {
                distances = new double[] { 88, 88, 88, 88, 88, 88, 88, 88, 88, 88, 88, 88, 88, 88 };
                for (int i = 0; i < 14; i++)
                {
                    contours[i] = new PointContour(0, 0);
                }

            }
            // Dispose the IVA_Data structure.
            ivaData.Dispose();
            // Return the palette type of the final image.
            return PaletteType.Binary;

        }

        public static PaletteType RectLeftOutCheck(VisionImage image)
        {
            contours = new PointContour[8];
            // Initialize the IVA_Data structure to pass results and coordinate systems.
            IVA_Data ivaData = new IVA_Data(16, 1);

            // Extract Color Plane
            using (VisionImage plane = new VisionImage(ImageType.U8, 7))
            {
                // Extract the red color plane and copy it to the main image.
                Algorithms.ExtractColorPlanes(image, ColorMode.Rgb, plane, null, null);
                Algorithms.Copy(plane, image);
            }

            // Creates a new, empty region of interest.
            Roi roi = new Roi();
            // Creates a new RectangleContour using the given values.
            RectangleContour vaRect = new RectangleContour(630, 1313, 1073, 416);
            roi.Add(vaRect);
            // MatchPattern Grayscale
            string dicpath = System.Windows.Forms.Application.StartupPath;
            string vaTemplateFile = dicpath + $"{ @"/ImageConfig/LeftRectPos.png"}";
            //string vaTemplateFile = "D:\\Desktop\\LeftRectPos.png";
            MatchingAlgorithm matchAlgorithm = MatchingAlgorithm.MatchGrayValuePyramid;
            float[] minAngleVals = { -10, 0 };
            float[] maxAngleVals = { 10, 0 };
            int[] advancedOptionsItems = { 100, 102, 106, 107, 108, 109, 114, 116, 117, 118, 111, 112, 113, 103, 104, 105 };
            double[] advancedOptionsValues = { 5, 10, 300, 0, 6, 1, 25, 0, 0, 0, 20, 10, 20, 1, 20, 0 };
            int numberAdvOptions = 16;
            int vaNumMatchesRequested = 1;
            float vaMinMatchScore = 700;
            pmResults = IVA_MatchPattern(image, ivaData, vaTemplateFile, matchAlgorithm, minAngleVals, maxAngleVals, advancedOptionsItems, advancedOptionsValues, numberAdvOptions, vaNumMatchesRequested, vaMinMatchScore, roi, 2);
            roi.Dispose();

            if (pmResults.Count == 1)
            {
                // Set Coordinate System
                int vaCoordSystemIndex = 0;
                int stepIndexOrigin = 2;
                int resultIndexOrigin = 1;
                int stepIndexAngle = 2;
                int resultIndexAngle = 3;
                double refSysOriginX = 1160.5;
                double refSysOriginY = 1500.5;
                double refSysAngle = 0;
                AxisOrientation refSysAxisOrientation = AxisOrientation.Direct;
                int vaCoordSystemType = 3;
                IVA_CoordSys(vaCoordSystemIndex, stepIndexOrigin, resultIndexOrigin, stepIndexAngle, resultIndexAngle, refSysOriginX, refSysOriginY, refSysAngle, refSysAxisOrientation, vaCoordSystemType, ivaData);

                // Manual Threshold
                Algorithms.Threshold(image, image, new Range(Position.Instance.Threshold_Left, 255), true, 1);

                // Advanced Morphology: Remove Border Objects - Eliminates particles touching the border of the image.
                Algorithms.RejectBorder(image, image, Connectivity.Connectivity8);

                // Basic Morphology - Applies morphological transformations to binary images.
                int[] vaCoefficients = { 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0 };
                StructuringElement vaStructElem = new StructuringElement(5, 5, vaCoefficients);
                vaStructElem.Shape = StructuringElementShape.Square;
                // Applies morphological transformations
                Algorithms.Morphology(image, image, MorphologyMethod.Open, vaStructElem);

                // Advanced Morphology: Remove Objects
                int[] vaCoefficients2 = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
                StructuringElement vaStructElem2 = new StructuringElement(3, 3, vaCoefficients2);
                vaStructElem.Shape = StructuringElementShape.Square;
                // Filters particles based on their size.
                Algorithms.RemoveParticle(image, image, 10, SizeToKeep.KeepLarge, Connectivity.Connectivity8, vaStructElem2);

                VisionImage newimage = new VisionImage(ImageType.U8, 7);
                Algorithms.FillHoles(image, newimage, Connectivity.Connectivity8);
                Collection<MeasurementType> vaPixelMeasurements = new Collection<MeasurementType>(new MeasurementType[] { MeasurementType.Area });
                Collection<MeasurementType> vaCalibratedMeasurements = new Collection<MeasurementType>(new MeasurementType[] { });
                IVA_Particle(newimage, Connectivity.Connectivity8, vaPixelMeasurements, vaCalibratedMeasurements, ivaData, 9, out vaParticleReport, out vaParticleReportCalibrated);

                double[,] area = vaParticleReport.PixelMeasurements;
                double Maxarea = 0;
                for (int i = 0; i < area.GetLength(0); i++)
                {
                    for (int j = 0; j < area.GetLength(1); j++)
                    {
                        if (area[i, j] > Maxarea)
                        {
                            Maxarea = area[i, j];
                        }
                    }
                }
                newimage.Dispose();

                if (Maxarea < 700000)
                {
                    distances = new double[] { 99, 99, 99, 99, 99, 99, 99, 99 };
                    for (int i = 0; i < 8; i++)
                    {
                        contours[i] = new PointContour(0, 0);
                    }
                    ivaData.Dispose();
                    return PaletteType.Binary;
                }

                double MaxDis1 = 0, MaxDis2 = 0, MaxDis3 = 0, MaxDis4 = 0, MaxDis5 = 0, MaxDis6 = 0, MaxDis7 = 0, MaxDis8 = 0;
                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi2 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter = new PointContour(1428, 381);
                    AnnulusContour vaOval = new AnnulusContour(vaCenter, 33, 78, 0, 90);
                    roi2.Add(vaOval);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex = 0;
                    Algorithms.TransformRoi(roi2, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex], ivaData.MeasurementSystems[coordSystemIndex]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray = { };
                    double[] vaConstraintMaxArray = { };
                    ConnectionConstraintType[] vaConstraintTypeArray = { };
                    ExtractContourReport vaExtractReport = IVA_ExtractContour(image, roi2, ExtractContourDirection.AnnulusOuterInner, vaCurveParams, vaConstraintTypeArray, vaConstraintMinArray, vaConstraintMaxArray, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay, vaEquationOverlay);
                    ComputeDistanceReport vaDistanceReport = Algorithms.ContourComputeDistances(image, image, 0);

                    roi2.Dispose(); for (int i = 0; i < vaDistanceReport.Distances.Count; i++)
                    {
                        if (vaDistanceReport.Distances[i].Distance > MaxDis1)
                        {
                            MaxDis1 = vaDistanceReport.Distances[i].Distance;

                            contours[0] = vaDistanceReport.Distances[i].CurrentPoint;
                        }
                    }
                    roi2.Dispose();
                }
                catch
                {
                    MaxDis1 = 0;

                    contours[0] = new PointContour(0, 0);
                }


                try
                {
                    // Creates a new, empty region of interest.
                    Roi roi3 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter2 = new PointContour(1482, 979.5);
                    RotatedRectangleContour vaRotatedRect = new RotatedRectangleContour(vaCenter2, 40, 1191, 0);
                    roi3.Add(vaRotatedRect);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex2 = 0;
                    Algorithms.TransformRoi(roi3, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex2], ivaData.MeasurementSystems[coordSystemIndex2]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams2 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray2 = { };
                    double[] vaConstraintMaxArray2 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray2 = { };
                    ExtractContourReport vaExtractReport2 = IVA_ExtractContour(image, roi3, ExtractContourDirection.RectRightLeft, vaCurveParams2, vaConstraintTypeArray2, vaConstraintMinArray2, vaConstraintMaxArray2, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay2 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay2 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay2, vaEquationOverlay2);
                    ComputeDistanceReport vaDistanceReport2 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport2.Distances.Count; i++)
                    {
                        if (vaDistanceReport2.Distances[i].Distance > MaxDis2)
                        {
                            MaxDis2 = vaDistanceReport2.Distances[i].Distance;

                            contours[1] = vaDistanceReport2.Distances[i].CurrentPoint;
                        }
                    }

                    roi3.Dispose();
                }
                catch
                {
                    MaxDis2 = 0;

                    contours[1] = new PointContour(0, 0);
                }

                try
                {

                    // Creates a new, empty region of interest.
                    Roi roi4 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter3 = new PointContour(1427, 1576);
                    AnnulusContour vaOval2 = new AnnulusContour(vaCenter3, 30, 72, 270, 0);
                    roi4.Add(vaOval2);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex3 = 0;
                    Algorithms.TransformRoi(roi4, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex3], ivaData.MeasurementSystems[coordSystemIndex3]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams3 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray3 = { };
                    double[] vaConstraintMaxArray3 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray3 = { };
                    ExtractContourReport vaExtractReport3 = IVA_ExtractContour(image, roi4, ExtractContourDirection.AnnulusOuterInner, vaCurveParams3, vaConstraintTypeArray3, vaConstraintMinArray3, vaConstraintMaxArray3, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay3 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay3 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport2 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay3, vaEquationOverlay3);
                    ComputeDistanceReport vaDistanceReport3 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport3.Distances.Count; i++)
                    {
                        if (vaDistanceReport3.Distances[i].Distance > MaxDis3)
                        {
                            MaxDis3 = vaDistanceReport3.Distances[i].Distance;

                            contours[2] = vaDistanceReport3.Distances[i].CurrentPoint;
                        }
                    }

                    roi4.Dispose();
                }
                catch
                {
                    MaxDis3 = 0;

                    contours[2] = new PointContour(0, 0);
                }

                try
                {

                    // Creates a new, empty region of interest.
                    Roi roi5 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter4 = new PointContour(1164.5, 1626.5);
                    RotatedRectangleContour vaRotatedRect2 = new RotatedRectangleContour(vaCenter4, 517, 45, 0);
                    roi5.Add(vaRotatedRect2);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex4 = 0;
                    Algorithms.TransformRoi(roi5, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex4], ivaData.MeasurementSystems[coordSystemIndex4]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams4 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.ContourTracing, 25, 15, 10, true);
                    double[] vaConstraintMinArray4 = { };
                    double[] vaConstraintMaxArray4 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray4 = { };
                    ExtractContourReport vaExtractReport4 = IVA_ExtractContour(image, roi5, ExtractContourDirection.RectBottomTop, vaCurveParams4, vaConstraintTypeArray4, vaConstraintMinArray4, vaConstraintMaxArray4, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay4 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay4 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport2 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay4, vaEquationOverlay4);
                    ComputeDistanceReport vaDistanceReport4 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport4.Distances.Count; i++)
                    {
                        if (vaDistanceReport4.Distances[i].Distance > MaxDis4)
                        {
                            MaxDis4 = vaDistanceReport4.Distances[i].Distance;

                            contours[3] = vaDistanceReport4.Distances[i].CurrentPoint;
                        }
                    }
                    roi5.Dispose();
                }
                catch
                {
                    MaxDis4 = 0;

                    contours[3] = new PointContour(0, 0);
                }


                try
                {

                    // Creates a new, empty region of interest.
                    Roi roi6 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter5 = new PointContour(903, 1568);
                    AnnulusContour vaOval3 = new AnnulusContour(vaCenter5, 34, 81, 180, 270);
                    roi6.Add(vaOval3);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex5 = 0;
                    Algorithms.TransformRoi(roi6, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex5], ivaData.MeasurementSystems[coordSystemIndex5]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams5 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray5 = { };
                    double[] vaConstraintMaxArray5 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray5 = { };
                    ExtractContourReport vaExtractReport5 = IVA_ExtractContour(image, roi6, ExtractContourDirection.AnnulusOuterInner, vaCurveParams5, vaConstraintTypeArray5, vaConstraintMinArray5, vaConstraintMaxArray5, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay5 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay5 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport3 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay5, vaEquationOverlay5);
                    ComputeDistanceReport vaDistanceReport5 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport5.Distances.Count; i++)
                    {
                        if (vaDistanceReport5.Distances[i].Distance > MaxDis5)
                        {
                            MaxDis5 = vaDistanceReport5.Distances[i].Distance;

                            contours[4] = vaDistanceReport5.Distances[i].CurrentPoint;
                        }
                    }
                    roi6.Dispose();
                }
                catch
                {
                    MaxDis5 = 0;

                    contours[4] = new PointContour(0, 0);
                }

                try
                {

                    // Creates a new, empty region of interest.
                    Roi roi7 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter6 = new PointContour(846.5, 975);
                    RotatedRectangleContour vaRotatedRect3 = new RotatedRectangleContour(vaCenter6, 51, 1180, 0);
                    roi7.Add(vaRotatedRect3);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex6 = 0;
                    Algorithms.TransformRoi(roi7, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex6], ivaData.MeasurementSystems[coordSystemIndex6]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams6 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray6 = { };
                    double[] vaConstraintMaxArray6 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray6 = { };
                    ExtractContourReport vaExtractReport6 = IVA_ExtractContour(image, roi7, ExtractContourDirection.RectLeftRight, vaCurveParams6, vaConstraintTypeArray6, vaConstraintMinArray6, vaConstraintMaxArray6, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay6 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay6 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport3 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay6, vaEquationOverlay6);
                    ComputeDistanceReport vaDistanceReport6 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport6.Distances.Count; i++)
                    {
                        if (vaDistanceReport6.Distances[i].Distance > MaxDis6)
                        {
                            MaxDis6 = vaDistanceReport6.Distances[i].Distance;

                            contours[5] = vaDistanceReport6.Distances[i].CurrentPoint;
                        }
                    }
                    roi7.Dispose();
                }
                catch
                {
                    MaxDis6 = 0;

                    contours[5] = new PointContour(0, 0);
                }


                try
                {

                    // Creates a new, empty region of interest.
                    Roi roi8 = new Roi();
                    // Creates a new AnnulusContour using the given values.
                    PointContour vaCenter7 = new PointContour(908, 381);
                    AnnulusContour vaOval4 = new AnnulusContour(vaCenter7, 37, 86, 90.744, 180);
                    roi8.Add(vaOval4);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex7 = 0;
                    Algorithms.TransformRoi(roi8, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex7], ivaData.MeasurementSystems[coordSystemIndex7]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams7 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray7 = { };
                    double[] vaConstraintMaxArray7 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray7 = { };
                    ExtractContourReport vaExtractReport7 = IVA_ExtractContour(image, roi8, ExtractContourDirection.AnnulusOuterInner, vaCurveParams7, vaConstraintTypeArray7, vaConstraintMinArray7, vaConstraintMaxArray7, ExtractContourSelection.Closest);
                    // Fit a circle to the contour
                    ContourOverlaySettings vaEquationOverlay7 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay7 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    PartialCircle vaCircleReport4 = Algorithms.ContourFitCircle(image, 30, true);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay7, vaEquationOverlay7);
                    ComputeDistanceReport vaDistanceReport7 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport7.Distances.Count; i++)
                    {
                        if (vaDistanceReport7.Distances[i].Distance > MaxDis7)
                        {
                            MaxDis7 = vaDistanceReport7.Distances[i].Distance;

                            contours[6] = vaDistanceReport7.Distances[i].CurrentPoint;
                        }
                    }
                    roi8.Dispose();
                }
                catch
                {
                    MaxDis7 = 0;

                    contours[6] = new PointContour(0, 0);
                }


                try
                {

                    // Creates a new, empty region of interest.
                    Roi roi9 = new Roi();
                    // Creates a new RotatedRectangleContour using the given values.
                    PointContour vaCenter8 = new PointContour(1168, 322.5);
                    RotatedRectangleContour vaRotatedRect4 = new RotatedRectangleContour(vaCenter8, 512, 45, 0);
                    roi9.Add(vaRotatedRect4);
                    // Reposition the region of interest based on the coordinate system.
                    int coordSystemIndex8 = 0;
                    Algorithms.TransformRoi(roi9, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex8], ivaData.MeasurementSystems[coordSystemIndex8]));
                    // Extract the contour edges from the image
                    CurveParameters vaCurveParams8 = new CurveParameters(ExtractionMode.NormalImage, 1, EdgeFilterSize.Normal, 25, 15, 10, true);
                    double[] vaConstraintMinArray8 = { };
                    double[] vaConstraintMaxArray8 = { };
                    ConnectionConstraintType[] vaConstraintTypeArray8 = { };
                    ExtractContourReport vaExtractReport8 = IVA_ExtractContour(image, roi9, ExtractContourDirection.RectTopBottom, vaCurveParams8, vaConstraintTypeArray8, vaConstraintMinArray8, vaConstraintMaxArray8, ExtractContourSelection.Closest);
                    // Fit a line to the contour
                    ContourOverlaySettings vaEquationOverlay8 = new ContourOverlaySettings(true, Rgb32Value.GreenColor, 1, true);
                    ContourOverlaySettings vaPointsOverlay8 = new ContourOverlaySettings(true, Rgb32Value.RedColor, 1, true);
                    ContourFitLineReport vaFitLineReport4 = Algorithms.ContourFitLine(image, 3);
                    Algorithms.ContourOverlay(image, image, vaPointsOverlay8, vaEquationOverlay8);
                    ComputeDistanceReport vaDistanceReport8 = Algorithms.ContourComputeDistances(image, image, 0);

                    for (int i = 0; i < vaDistanceReport8.Distances.Count; i++)
                    {
                        if (vaDistanceReport8.Distances[i].Distance > MaxDis8)
                        {
                            MaxDis8 = vaDistanceReport8.Distances[i].Distance;

                            contours[7] = vaDistanceReport8.Distances[i].CurrentPoint;
                        }
                    }
                    roi9.Dispose();
                }
                catch
                {
                    MaxDis8 = 0;

                    contours[7] = new PointContour(0, 0);
                }
                distances = new double[] { MaxDis1, MaxDis2, MaxDis3, MaxDis4, MaxDis5, MaxDis6, MaxDis7, MaxDis8 };
            }
            else
            {
                distances = new double[] { 88, 88, 88, 88, 88, 88, 88, 88 };
                for (int i = 0; i < 8; i++)
                {
                    contours[i] = new PointContour(0, 0);
                }

            }

            // Dispose the IVA_Data structure.
            ivaData.Dispose();
            // Return the palette type of the final image.
            return PaletteType.Binary;

        }

    }
}

