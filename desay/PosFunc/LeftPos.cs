using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using NationalInstruments.Vision.WindowsForms;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Analysis;
using Vision_Assistant.Utilities;
using System.IO;
using desay.ProductData;

namespace Vision_Assistant
{
    static class LeftPos
    {
        public static Collection<PatternMatchReport> pmResults;
        public static FindEdgeReport vaEdgeReport;
        public static FindEdgeReport vaEdgeReport2;
        public static FindEdgeReport vaEdgeReport3;
        public static FindEdgeReport vaEdgeReport4;
        public static PointContour caliperIntersection;
        public static PointContour caliperIntersection2;
        public static PointContour caliperIntersection3;
        public static PointContour caliperIntersection4;
        public static string LeftCali;
        public static string[] LeftCaliArrary;

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

        private static FindEdgeReport IVA_FindEdge(VisionImage image,
                                                Roi roi,
                                                RakeDirection direction,
                                                EdgeOptions options,
                                                StraightEdgeOptions straightEdgeOptions,
                                                IVA_Data ivaData,
                                                int stepIndex)
        {

            // First, delete all the results of this step (from a previous iteration)
            Functions.IVA_DisposeStepResults(ivaData, stepIndex);

            // Find the Edge
            FindEdgeOptions edgeOptions = new FindEdgeOptions(direction);
            edgeOptions.EdgeOptions = options;
            edgeOptions.StraightEdgeOptions = straightEdgeOptions;
            FindEdgeReport lineReport = new FindEdgeReport();
            lineReport = Algorithms.FindEdge(image, roi, edgeOptions);

            // If there was at least one line, get data
            if (lineReport.StraightEdges.Count >= 1)
            {
                // Store the results in the data structure.
                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 1.X Position (Pix.)", lineReport.StraightEdges[0].StraightEdge.Start.X));
                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 1.Y Position (Pix.)", lineReport.StraightEdges[0].StraightEdge.Start.Y));
                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 2.X Position (Pix.)", lineReport.StraightEdges[0].StraightEdge.End.X));
                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 2.Y Position (Pix.)", lineReport.StraightEdges[0].StraightEdge.End.Y));
                if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                {
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 1.X Position (World)", lineReport.StraightEdges[0].CalibratedStraightEdge.Start.X));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 1.Y Position (World)", lineReport.StraightEdges[0].CalibratedStraightEdge.Start.Y));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 2.X Position (World)", lineReport.StraightEdges[0].CalibratedStraightEdge.End.X));
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Point 2.Y Position (World)", lineReport.StraightEdges[0].CalibratedStraightEdge.End.Y));
                }

                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Angle", lineReport.StraightEdges[0].Angle));
                if ((image.InfoTypes & InfoTypes.Calibration) != 0)
                {
                    ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Angle (World)", lineReport.StraightEdges[0].CalibratedAngle));
                }
            }
            return lineReport;
        }

        private static Collection<PointContour> IVA_GetIntersection(VisionImage image,
                                                                    IVA_Data ivaData,
                                                                    int stepIndex,
                                                                    int stepIndex1,
                                                                    int resultIndex1,
                                                                    int stepIndex2,
                                                                    int resultIndex2,
                                                                    int stepIndex3,
                                                                    int resultIndex3,
                                                                    int stepIndex4,
                                                                    int resultIndex4)

        {

            // Caliper: Lines Intersection
            // Computes the intersection point between two lines.
            PointContour point1 = Functions.IVA_GetPoint(ivaData, stepIndex1, resultIndex1);
            PointContour point2 = Functions.IVA_GetPoint(ivaData, stepIndex2, resultIndex2);
            PointContour point3 = Functions.IVA_GetPoint(ivaData, stepIndex3, resultIndex3);
            PointContour point4 = Functions.IVA_GetPoint(ivaData, stepIndex4, resultIndex4);

            LineContour line1 = new LineContour(point1, point2);
            LineContour line2 = new LineContour(point3, point4);

            Collection<PointContour> intersectionPoint = new Collection<PointContour>();
            intersectionPoint.Add(Algorithms.FindIntersectionPoint(line1, line2));

            // Store the results in the data structure.
            ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Intersection Point X Position (Pix.)", intersectionPoint[0].X));
            ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Intersection Point X Position (Pix.)", intersectionPoint[0].Y));

            // If the image is calibrated, compute the real world position.
            if ((image.InfoTypes & InfoTypes.Calibration) != 0)
            {
                CoordinatesReport realWorldPosition = Algorithms.ConvertPixelToRealWorldCoordinates(image, intersectionPoint);
                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Intersection Point X Position (Calibrated)", realWorldPosition.Points[0].X));
                ivaData.stepResults[stepIndex].results.Add(new IVA_Result("Intersection Point X Position (Calibrated)", realWorldPosition.Points[0].Y));
                intersectionPoint.Add(realWorldPosition.Points[0]);
            }

            return intersectionPoint;
        }


        public static PaletteType ProcessImage(VisionImage image, Point pointoffset)
        {
            // Initialize the IVA_Data structure to pass results and coordinate systems.
            IVA_Data ivaData = new IVA_Data(12, 1);

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
            RectangleContour vaRect = new RectangleContour(623, 689, 1091, 647);
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
                double refSysOriginX = 1348.5;
                double refSysOriginY = 876.5;
                double refSysAngle = 0;
                AxisOrientation refSysAxisOrientation = AxisOrientation.Direct;
                int vaCoordSystemType = 3;
                IVA_CoordSys(vaCoordSystemIndex, stepIndexOrigin, resultIndexOrigin, stepIndexAngle, resultIndexAngle, refSysOriginX, refSysOriginY, refSysAngle, refSysAxisOrientation, vaCoordSystemType, ivaData);

                // Creates a new, empty region of interest.
                Roi roi2 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter = new PointContour(785, 1013.5);
                RotatedRectangleContour vaRotatedRect = new RotatedRectangleContour(vaCenter, 46, 645, 0);
                roi2.Add(vaRotatedRect);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex = 0;
                Algorithms.TransformRoi(roi2, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex], ivaData.MeasurementSystems[coordSystemIndex]));
                // Find Straight Edge
                EdgeOptions vaOptions = new EdgeOptions();
                vaOptions.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions.KernelSize = 3;
                vaOptions.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions.Width = 1;
                StraightEdgeOptions vaStraightEdgeOptions = new StraightEdgeOptions();
                vaStraightEdgeOptions.AngleRange = 45;
                vaStraightEdgeOptions.AngleTolerance = 1;
                vaStraightEdgeOptions.HoughIterations = 5;
                vaStraightEdgeOptions.MinimumCoverage = 25;
                vaStraightEdgeOptions.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions.NumberOfLines = 1;
                vaStraightEdgeOptions.Orientation = 0;
                Range vaRange = new Range(0, 1000);
                vaStraightEdgeOptions.ScoreRange = vaRange;
                vaStraightEdgeOptions.StepSize = 35;
                vaStraightEdgeOptions.SearchMode = StraightEdgeSearchMode.BestRakeEdges;

                vaEdgeReport = IVA_FindEdge(image, roi2, RakeDirection.RightToLeft, vaOptions, vaStraightEdgeOptions, ivaData, 4);

                roi2.Dispose();

                // Creates a new, empty region of interest.
                Roi roi3 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter2 = new PointContour(1546.5, 1029);
                RotatedRectangleContour vaRotatedRect2 = new RotatedRectangleContour(vaCenter2, 43, 682, 0);
                roi3.Add(vaRotatedRect2);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex2 = 0;
                Algorithms.TransformRoi(roi3, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex2], ivaData.MeasurementSystems[coordSystemIndex2]));
                // Find Straight Edge
                EdgeOptions vaOptions2 = new EdgeOptions();
                vaOptions2.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions2.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions2.KernelSize = 3;
                vaOptions2.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions2.Polarity = EdgePolaritySearchMode.All;
                vaOptions2.Width = 1;
                StraightEdgeOptions vaStraightEdgeOptions2 = new StraightEdgeOptions();
                vaStraightEdgeOptions2.AngleRange = 45;
                vaStraightEdgeOptions2.AngleTolerance = 1;
                vaStraightEdgeOptions2.HoughIterations = 5;
                vaStraightEdgeOptions2.MinimumCoverage = 25;
                vaStraightEdgeOptions2.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions2.NumberOfLines = 1;
                vaStraightEdgeOptions2.Orientation = 0;
                Range vaRange2 = new Range(0, 1000);
                vaStraightEdgeOptions2.ScoreRange = vaRange2;
                vaStraightEdgeOptions2.StepSize = 35;
                vaStraightEdgeOptions2.SearchMode = StraightEdgeSearchMode.BestRakeEdges;

                vaEdgeReport2 = IVA_FindEdge(image, roi3, RakeDirection.LeftToRight, vaOptions2, vaStraightEdgeOptions2, ivaData, 5);

                roi3.Dispose();

                // Creates a new, empty region of interest.
                Roi roi4 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter3 = new PointContour(1186.5, 1906);
                RotatedRectangleContour vaRotatedRect3 = new RotatedRectangleContour(vaCenter3, 455, 62, 0);
                roi4.Add(vaRotatedRect3);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex3 = 0;
                Algorithms.TransformRoi(roi4, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex3], ivaData.MeasurementSystems[coordSystemIndex3]));
                // Find Straight Edge
                EdgeOptions vaOptions3 = new EdgeOptions();
                vaOptions3.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions3.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions3.KernelSize = 3;
                vaOptions3.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions3.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions3.Width = 1;
                StraightEdgeOptions vaStraightEdgeOptions3 = new StraightEdgeOptions();
                vaStraightEdgeOptions3.AngleRange = 45;
                vaStraightEdgeOptions3.AngleTolerance = 1;
                vaStraightEdgeOptions3.HoughIterations = 5;
                vaStraightEdgeOptions3.MinimumCoverage = 25;
                vaStraightEdgeOptions3.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions3.NumberOfLines = 1;
                vaStraightEdgeOptions3.Orientation = 0;
                Range vaRange3 = new Range(0, 1000);
                vaStraightEdgeOptions3.ScoreRange = vaRange3;
                vaStraightEdgeOptions3.StepSize = 20;
                vaStraightEdgeOptions3.SearchMode = StraightEdgeSearchMode.BestRakeEdges;

                vaEdgeReport3 = IVA_FindEdge(image, roi4, RakeDirection.TopToBottom, vaOptions3, vaStraightEdgeOptions3, ivaData, 6);

                roi4.Dispose();

                // Creates a new, empty region of interest.
                Roi roi5 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter4 = new PointContour(1183.5, 97);
                RotatedRectangleContour vaRotatedRect4 = new RotatedRectangleContour(vaCenter4, 517, 70, 0);
                roi5.Add(vaRotatedRect4);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex4 = 0;
                Algorithms.TransformRoi(roi5, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex4], ivaData.MeasurementSystems[coordSystemIndex4]));
                // Find Straight Edge
                EdgeOptions vaOptions4 = new EdgeOptions();
                vaOptions4.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions4.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions4.KernelSize = 3;
                vaOptions4.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions4.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions4.Width = 1;
                StraightEdgeOptions vaStraightEdgeOptions4 = new StraightEdgeOptions();
                vaStraightEdgeOptions4.AngleRange = 45;
                vaStraightEdgeOptions4.AngleTolerance = 1;
                vaStraightEdgeOptions4.HoughIterations = 5;
                vaStraightEdgeOptions4.MinimumCoverage = 25;
                vaStraightEdgeOptions4.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions4.NumberOfLines = 1;
                vaStraightEdgeOptions4.Orientation = 0;
                Range vaRange4 = new Range(0, 1000);
                vaStraightEdgeOptions4.ScoreRange = vaRange4;
                vaStraightEdgeOptions4.StepSize = 35;
                vaStraightEdgeOptions4.SearchMode = StraightEdgeSearchMode.BestRakeEdges;

                vaEdgeReport4 = IVA_FindEdge(image, roi5, RakeDirection.BottomToTop, vaOptions4, vaStraightEdgeOptions4, ivaData, 7);

                roi5.Dispose();

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 8);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection = IVA_GetIntersection(image, ivaData, 8, 7, 0, 7, 2, 5, 0, 5, 2);
                caliperIntersection = vaIntersection[0];

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 9);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection2 = IVA_GetIntersection(image, ivaData, 9, 5, 0, 5, 2, 6, 2, 6, 0);
                caliperIntersection2 = vaIntersection2[0];

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 10);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection3 = IVA_GetIntersection(image, ivaData, 10, 6, 2, 6, 0, 4, 2, 4, 0);
                caliperIntersection3 = vaIntersection3[0];

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 11);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection4 = IVA_GetIntersection(image, ivaData, 11, 4, 0, 4, 2, 7, 0, 7, 2);
                caliperIntersection4 = vaIntersection4[0];

                //计算每个角的偏差
                string str1 = Math.Round((-caliperIntersection.X - pointoffset.X + Position.Instance.SpecLeftPos_X[0]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[0]) / 96, 3).ToString() + ";";
                string str2 = Math.Round((-caliperIntersection2.X - pointoffset.X + Position.Instance.SpecLeftPos_X[1]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection2.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[1]) / 96, 3).ToString() + ";";
                string str3 = Math.Round((-caliperIntersection3.X - pointoffset.X + Position.Instance.SpecLeftPos_X[2]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection3.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[2]) / 96, 3).ToString() + ";";
                string str4 = Math.Round((-caliperIntersection4.X - pointoffset.X + Position.Instance.SpecLeftPos_X[3]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection4.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[3]) / 96, 3).ToString();
                LeftCali = str1 + str2 + str3 + str4;
                LeftCaliArrary = new string[] { str1, str2, str3, str4 };
            }
            else
            {
                LeftCali = "0;0;0;0;0;0;0;0";
                LeftCaliArrary = new string[] { "0;0", "0;0", "0;0", "0;0" };
            }

            // Dispose the IVA_Data structure.
            ivaData.Dispose();

            // Return the palette type of the final image.
            return PaletteType.Gray;

        }

        public static PaletteType RectLeftPos(VisionImage image, Point pointoffset)
        {

            // Initialize the IVA_Data structure to pass results and coordinate systems.
            IVA_Data ivaData = new IVA_Data(12, 1);

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

                // Creates a new, empty region of interest.
                Roi roi2 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter = new PointContour(789, 965.5);
                RotatedRectangleContour vaRotatedRect = new RotatedRectangleContour(vaCenter, 72, 1119, 0);
                roi2.Add(vaRotatedRect);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex = 0;
                Algorithms.TransformRoi(roi2, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex], ivaData.MeasurementSystems[coordSystemIndex]));
                // Find Straight Edge
                EdgeOptions vaOptions = new EdgeOptions();
                vaOptions.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions.KernelSize = 9;
                vaOptions.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions.Width = 5;
                StraightEdgeOptions vaStraightEdgeOptions = new StraightEdgeOptions();
                vaStraightEdgeOptions.AngleRange = 45;
                vaStraightEdgeOptions.AngleTolerance = 1;
                vaStraightEdgeOptions.HoughIterations = 5;
                vaStraightEdgeOptions.MinimumCoverage = 25;
                vaStraightEdgeOptions.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions.NumberOfLines = 1;
                vaStraightEdgeOptions.Orientation = 0;
                Range vaRange = new Range(0, 1000);
                vaStraightEdgeOptions.ScoreRange = vaRange;
                vaStraightEdgeOptions.StepSize = 20;
                vaStraightEdgeOptions.SearchMode = StraightEdgeSearchMode.FirstRakeEdges;

                vaEdgeReport = IVA_FindEdge(image, roi2, RakeDirection.LeftToRight, vaOptions, vaStraightEdgeOptions, ivaData, 4);

                roi2.Dispose();

                // Creates a new, empty region of interest.
                Roi roi3 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter2 = new PointContour(1162.5, 263);
                RotatedRectangleContour vaRotatedRect2 = new RotatedRectangleContour(vaCenter2, 595, 78, 0);
                roi3.Add(vaRotatedRect2);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex2 = 0;
                Algorithms.TransformRoi(roi3, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex2], ivaData.MeasurementSystems[coordSystemIndex2]));
                // Find Straight Edge
                EdgeOptions vaOptions2 = new EdgeOptions();
                vaOptions2.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions2.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions2.KernelSize = 9;
                vaOptions2.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions2.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions2.Width = 9;
                StraightEdgeOptions vaStraightEdgeOptions2 = new StraightEdgeOptions();
                vaStraightEdgeOptions2.AngleRange = 45;
                vaStraightEdgeOptions2.AngleTolerance = 1;
                vaStraightEdgeOptions2.HoughIterations = 5;
                vaStraightEdgeOptions2.MinimumCoverage = 25;
                vaStraightEdgeOptions2.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions2.NumberOfLines = 1;
                vaStraightEdgeOptions2.Orientation = 0;
                Range vaRange2 = new Range(0, 1000);
                vaStraightEdgeOptions2.ScoreRange = vaRange2;
                vaStraightEdgeOptions2.StepSize = 20;
                vaStraightEdgeOptions2.SearchMode = StraightEdgeSearchMode.FirstRakeEdges;

                vaEdgeReport2 = IVA_FindEdge(image, roi3, RakeDirection.TopToBottom, vaOptions2, vaStraightEdgeOptions2, ivaData, 5);

                roi3.Dispose();

                // Creates a new, empty region of interest.
                Roi roi4 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter3 = new PointContour(1530, 968.5);
                RotatedRectangleContour vaRotatedRect3 = new RotatedRectangleContour(vaCenter3, 78, 1137, 0);
                roi4.Add(vaRotatedRect3);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex3 = 0;
                Algorithms.TransformRoi(roi4, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex3], ivaData.MeasurementSystems[coordSystemIndex3]));
                // Find Straight Edge
                EdgeOptions vaOptions3 = new EdgeOptions();
                vaOptions3.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions3.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions3.KernelSize = 9;
                vaOptions3.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions3.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions3.Width = 9;
                StraightEdgeOptions vaStraightEdgeOptions3 = new StraightEdgeOptions();
                vaStraightEdgeOptions3.AngleRange = 45;
                vaStraightEdgeOptions3.AngleTolerance = 1;
                vaStraightEdgeOptions3.HoughIterations = 5;
                vaStraightEdgeOptions3.MinimumCoverage = 25;
                vaStraightEdgeOptions3.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions3.NumberOfLines = 1;
                vaStraightEdgeOptions3.Orientation = 0;
                Range vaRange3 = new Range(0, 1000);
                vaStraightEdgeOptions3.ScoreRange = vaRange3;
                vaStraightEdgeOptions3.StepSize = 20;
                vaStraightEdgeOptions3.SearchMode = StraightEdgeSearchMode.FirstRakeEdges;

                vaEdgeReport3 = IVA_FindEdge(image, roi4, RakeDirection.RightToLeft, vaOptions3, vaStraightEdgeOptions3, ivaData, 6);

                roi4.Dispose();

                // Creates a new, empty region of interest.
                Roi roi5 = new Roi();
                // Creates a new RotatedRectangleContour using the given values.
                PointContour vaCenter4 = new PointContour(1171.5, 1691.5);
                RotatedRectangleContour vaRotatedRect4 = new RotatedRectangleContour(vaCenter4, 543, 75, 0);
                roi5.Add(vaRotatedRect4);
                // Reposition the region of interest based on the coordinate system.
                int coordSystemIndex4 = 0;
                Algorithms.TransformRoi(roi5, new CoordinateTransform(ivaData.baseCoordinateSystems[coordSystemIndex4], ivaData.MeasurementSystems[coordSystemIndex4]));
                // Find Straight Edge
                EdgeOptions vaOptions4 = new EdgeOptions();
                vaOptions4.ColumnProcessingMode = ColumnProcessingMode.Average;
                vaOptions4.InterpolationType = InterpolationMethod.Bilinear;
                vaOptions4.KernelSize = 11;
                vaOptions4.MinimumThreshold = Position.Instance.EdgeThreshold_Left;
                vaOptions4.Polarity = EdgePolaritySearchMode.Falling;
                vaOptions4.Width = 9;
                StraightEdgeOptions vaStraightEdgeOptions4 = new StraightEdgeOptions();
                vaStraightEdgeOptions4.AngleRange = 45;
                vaStraightEdgeOptions4.AngleTolerance = 1;
                vaStraightEdgeOptions4.HoughIterations = 5;
                vaStraightEdgeOptions4.MinimumCoverage = 25;
                vaStraightEdgeOptions4.MinimumSignalToNoiseRatio = 0;
                vaStraightEdgeOptions4.NumberOfLines = 1;
                vaStraightEdgeOptions4.Orientation = 0;
                Range vaRange4 = new Range(0, 1000);
                vaStraightEdgeOptions4.ScoreRange = vaRange4;
                vaStraightEdgeOptions4.StepSize = 20;
                vaStraightEdgeOptions4.SearchMode = StraightEdgeSearchMode.FirstRakeEdges;

                vaEdgeReport4 = IVA_FindEdge(image, roi5, RakeDirection.BottomToTop, vaOptions4, vaStraightEdgeOptions4, ivaData, 7);

                roi5.Dispose();

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 8);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection = IVA_GetIntersection(image, ivaData, 8, 5, 0, 5, 2, 6, 0, 6, 2);
                caliperIntersection = vaIntersection[0];

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 9);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection2 = IVA_GetIntersection(image, ivaData, 9, 6, 0, 6, 2, 7, 0, 7, 2);
                caliperIntersection2 = vaIntersection2[0];

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 10);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection3 = IVA_GetIntersection(image, ivaData, 10, 4, 0, 4, 2, 7, 0, 7, 2);
                caliperIntersection3 = vaIntersection3[0];

                // Caliper
                // Delete all the results of this step (from a previous iteration)
                Functions.IVA_DisposeStepResults(ivaData, 11);

                // Computes the vaIntersection point between two lines.
                Collection<PointContour> vaIntersection4 = IVA_GetIntersection(image, ivaData, 11, 4, 0, 4, 2, 5, 0, 5, 2);
                caliperIntersection4 = vaIntersection4[0];

                //计算每个角的偏差
                string str1 = Math.Round((-caliperIntersection.X - pointoffset.X + Position.Instance.SpecLeftPos_X[0]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[0]) / 96, 3).ToString() + ";";
                string str2 = Math.Round((-caliperIntersection2.X - pointoffset.X + Position.Instance.SpecLeftPos_X[1]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection2.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[1]) / 96, 3).ToString() + ";";
                string str3 = Math.Round((-caliperIntersection3.X - pointoffset.X + Position.Instance.SpecLeftPos_X[2]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection3.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[2]) / 96, 3).ToString() + ";";
                string str4 = Math.Round((-caliperIntersection4.X - pointoffset.X + Position.Instance.SpecLeftPos_X[3]) / 96, 3).ToString() + ";" + Math.Round((-caliperIntersection4.Y - pointoffset.Y + Position.Instance.SpecLeftPos_Y[3]) / 96, 3).ToString();
                LeftCali = str1 + str2 + str3 + str4;
                LeftCaliArrary = new string[] { str1, str2, str3, str4 };

            }
            else
            {
                LeftCali = "0;0;0;0;0;0;0;0";
                LeftCaliArrary = new string[] { "0;0", "0;0", "0;0", "0;0" };
            }

            // Dispose the IVA_Data structure.
            ivaData.Dispose();

            // Return the palette type of the final image.
            return PaletteType.Gray;

        }

    }
}

