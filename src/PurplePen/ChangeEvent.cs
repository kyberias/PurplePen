/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

using PurplePen.MapModel;

namespace PurplePen
{
    /// <summary>
    /// This class has static methods for changing the event database in useful ways.
    /// Note that it never involves the undo manager; you must begin a command before calling
    /// one of these methods.
    /// You must ensure that your edit is correct and meaningful before calling these methods.
    /// </summary>
    static class ChangeEvent
    {
        // Change one of the symbols associated with a control. symbolNumber is a column numnber, 0=C, 1=D, etc.
        // newSymbol is the string id of the new symbol to put there. Use null as the newSymbol to indicate
        // no symbol there.
        public static void ChangeDescriptionSymbol(EventDB eventDB, Id<ControlPoint> controlId, int symbolNumber, string newSymbol)
        {
            ControlPoint controlPoint = eventDB.GetControl(controlId);

            controlPoint = (ControlPoint)controlPoint.Clone();
            controlPoint.symbolIds[symbolNumber] = newSymbol;
            if (symbolNumber == 3)
                controlPoint.columnFText = null;        // if a symbol is put in column F, then no text can be there.

            eventDB.ReplaceControlPoint(controlId, controlPoint);
        }

        // Change the text in colum F for a control. If a symbol is there, it is removed.Null means no text or symbol.
        public static void ChangeColumnFText(EventDB eventDB, Id<ControlPoint> controlId, string newText)
        {
            ControlPoint controlPoint = eventDB.GetControl(controlId);

            if (newText == "")
                newText = null;         // empty string is equivalent to null.

            controlPoint = (ControlPoint) controlPoint.Clone();
            controlPoint.symbolIds[3] = null;
            controlPoint.columnFText = newText;

            eventDB.ReplaceControlPoint(controlId, controlPoint);
        }

        // Change the code for a control. Does not attempt to validate the code -- it may even be a duplicate,
        // which is useful if renaming multiple codes in a single go. The control must be a normal control.
        public static void ChangeCode(EventDB eventDB, Id<ControlPoint> controlId, string newCode)
        {
            Debug.Assert(newCode != null);

            ControlPoint controlPoint = eventDB.GetControl(controlId);
            Debug.Assert(controlPoint.kind == ControlPointKind.Normal);

            controlPoint = (ControlPoint) controlPoint.Clone();
            controlPoint.code = newCode;

            eventDB.ReplaceControlPoint(controlId, controlPoint);
        }

        // Change a text line associated with a course control
        public static void ChangeTextLine(EventDB eventDB, Id<CourseControl> courseControlId, string textLine, bool above)
        {
            CourseControl courseControl = eventDB.GetCourseControl(courseControlId);

            courseControl = (CourseControl) courseControl.Clone();
            if (textLine == "")
                textLine = null;
            if (above)
                courseControl.descTextBefore = textLine;
            else
                courseControl.descTextAfter = textLine;

            eventDB.ReplaceCourseControl(courseControlId, courseControl);
        }

        // Change a text line associated with a control
        public static void ChangeTextLine(EventDB eventDB, Id<ControlPoint> controlId, string textLine, bool above)
        {
            ControlPoint control = eventDB.GetControl(controlId);

            control = (ControlPoint) control.Clone();
            if (textLine == "")
                textLine = null;
            if (above)
                control.descTextBefore = textLine;
            else
                control.descTextAfter = textLine;

            eventDB.ReplaceControlPoint(controlId, control);
        }

        // Change the control associated with a course control. Used when typing in a new existing code 
        // directly into a description sheet. Must be a normal control.
        public static void ChangeControl(EventDB eventDB, Id<CourseControl> courseControlId, Id<ControlPoint> newControlId)
        {
            Debug.Assert(newControlId.IsNotNone);

            CourseControl courseControl = eventDB.GetCourseControl(courseControlId);
            Debug.Assert(eventDB.GetControl(courseControl.control).kind == ControlPointKind.Normal);
            Debug.Assert(eventDB.GetControl(newControlId).kind == ControlPointKind.Normal);

            courseControl = (CourseControl) courseControl.Clone();
            courseControl.control = newControlId;
            courseControl.customNumberPlacement = false;   // any custome number placement is no longer valid

            eventDB.ReplaceCourseControl(courseControlId, courseControl);
        }

        // Change the score for a course-control. Does not validate the score. Use 0 for no score.
        public static void ChangeScore(EventDB eventDB, Id<CourseControl> courseControlId, int newScore)
        {
            CourseControl courseControl = eventDB.GetCourseControl(courseControlId);

            courseControl = (CourseControl)courseControl.Clone();
            courseControl.points = newScore;

            eventDB.ReplaceCourseControl(courseControlId, courseControl);
        }

        // Change the number location for a course-control. If customLocation is false, puts the number location as automaticLocation
        public static void ChangeNumberLocation(EventDB eventDB, Id<CourseControl> courseControlId, bool customLocation, PointF newLocation)
        {
            CourseControl courseControl = eventDB.GetCourseControl(courseControlId);
            ControlPoint control = eventDB.GetControl(courseControl.control);

            courseControl = (CourseControl) courseControl.Clone();
            courseControl.customNumberPlacement = customLocation;
            if (customLocation) {
                // Number locations are stored as a delta from the center of the control circle.
                courseControl.numberDeltaX = newLocation.X - control.location.X;
                courseControl.numberDeltaY = newLocation.Y - control.location.Y;
            }

            eventDB.ReplaceCourseControl(courseControlId, courseControl);
        }

        // Change the number location for a control in the all controls collection. If customLocation is false, puts the number location at the angle specified.
        public static void ChangeAllControlsCodeLocation(EventDB eventDB, Id<ControlPoint> controlId, bool customLocation, float newAngle)
        {
            ControlPoint controlPoint = eventDB.GetControl(controlId);

            controlPoint = (ControlPoint) controlPoint.Clone();
            controlPoint.customCodeLocation = customLocation;
            controlPoint.codeLocationAngle = newAngle;

            eventDB.ReplaceControlPoint(controlId, controlPoint);
        }

        // Change the event title. Seperate lines with "|".
        public static void ChangeEventTitle(EventDB eventDB, string newTitle)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.title = newTitle;

            eventDB.ChangeEvent(e);
        }

        // Change the map scale.
        public static void ChangeMapScale(EventDB eventDB, float newScale)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.mapScale = newScale;

            eventDB.ChangeEvent(e);
        }

        // Change the description language
        public static void ChangeDescriptionLanguage(EventDB eventDB, string newLangId)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.descriptionLangId = newLangId;

            eventDB.ChangeEvent(e);
        }

        // Change the auto numbering options.
        public static void ChangeAutoNumbering(EventDB eventDB, int startCode, bool disallowInvertibleCodes)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.firstControlCode = startCode;
            e.disallowInvertibleCodes = disallowInvertibleCodes;

            eventDB.ChangeEvent(e);
        }

        // Renumber all controls according the current start code/invertible code.
        // Rather than change every control, any code that will continue to be used remains assigned to the same control id. Controls getting
        // new codes are assigned in code order, to new codes in code order.
        public static void AutoRenumberControls(EventDB eventDB)
        {
            Event e = eventDB.GetEvent();

            Dictionary<string, bool> newCodes = new Dictionary<string,bool>(); // dictionary of new codes and if we still need to assign them
            Dictionary<string, Id<ControlPoint>> codeToControl = new Dictionary<string, Id<ControlPoint>>(); // dictionary mapping current codes to control ids.

            // Initialize the newCodes and codeToControl data structures
            int newCode = e.firstControlCode;
            string newCodeString = newCode.ToString();
            foreach (Id<ControlPoint> controlId in eventDB.AllControlPointIds) {
                ControlPoint control = eventDB.GetControl(controlId);
                string reason;
                bool legal;

                if (control.kind == ControlPointKind.Normal) {
                    // Add to codeToControl mapping dictionary.
                    codeToControl.Add(control.code, controlId);

                    // Add a new code to the new code dictionary
                    newCodes.Add(newCodeString, true);
                    do {
                        ++newCode;
                        if (newCode >= 1000)
                            newCode = 31;
                        newCodeString = newCode.ToString();

                        // Is the new code legal and preferred?
                        legal = QueryEvent.IsPreferredControlCode(eventDB, newCodeString, out reason);
                    } while (!legal || reason != null);  // filters out invertible (if selected in the event).
                }
            }

            // Remove controls from the codeToControl dictionary that have codes we will continue to use, and mark those 
            // codes are assigned.
            List<string> newCodeStrings = new List<string>(newCodes.Keys);
            foreach (string code in newCodeStrings) {
                if (codeToControl.ContainsKey(code)) {
                    codeToControl.Remove(code);
                    newCodes[code] = false;
                }
            }

            // Put the codeToControl dictionary into a list and sort it.
            List<KeyValuePair<string, Id<ControlPoint>>> codeToControlList = new List<KeyValuePair<string, Id<ControlPoint>>>(codeToControl);
            codeToControlList.Sort(delegate(KeyValuePair<string, Id<ControlPoint>> pair1, KeyValuePair<string, Id<ControlPoint>> pair2) {
                return Util.CompareCodes(pair1.Key, pair2.Key);
            });

            // Put the codes still to be assigned into a list and sort it.
            List<string> newCodeList = new List<string>();
            foreach (string code in newCodes.Keys)
                if (newCodes[code])
                    newCodeList.Add(code);
            newCodeList.Sort(Util.CompareCodes);

            // Assign new codes.
            Debug.Assert(codeToControlList.Count == newCodeList.Count);
            for (int i = 0; i < codeToControlList.Count; ++i) {
                ChangeEvent.ChangeCode(eventDB, codeToControlList[i].Value, newCodeList[i]);
            }
        }

        // Change the attributes of a all controls display.
        public static void ChangeAllControlsProperties(EventDB eventDB, float printScale, DescriptionKind descriptionKind)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.allControlsPrintScale = printScale;
            e.allControlsDescKind = descriptionKind;

            eventDB.ChangeEvent(e);
        }

        // Change the course name.
        public static void ChangeCourseName(EventDB eventDB, Id<Course> courseId, string newName)
        {
            Course course = eventDB.GetCourse(courseId);

            course = (Course)course.Clone();
            course.name = newName;

            eventDB.ReplaceCourse(courseId, course);
        }

        // Change the course climb.
        public static void ChangeCourseClimb(EventDB eventDB, Id<Course> courseId, float newClimb)
        {
            Course course = eventDB.GetCourse(courseId);

            course = (Course)course.Clone();
            course.climb = newClimb;

            eventDB.ReplaceCourse(courseId, course);
        }

        // Change the course secondary title. Use "" or null to remove the secondary title.
        public static void ChangeCourseSecondaryTitle(EventDB eventDB, Id<Course> courseId, string newTitle)
        {
            if (newTitle == "")
                newTitle = null;
            Course course = eventDB.GetCourse(courseId);

            course = (Course)course.Clone();
            course.secondaryTitle = newTitle;

            eventDB.ReplaceCourse(courseId, course);
        }

        // Change the course print area. Use CourseId.None to change the all controls print area. Use empty
        // rectangle to switch back to the default print area.
        public static void ChangeCoursePrintArea(EventDB eventDB, Id<Course> courseId, RectangleF printArea)
        {
            if (courseId.IsNone) {
                Event e = eventDB.GetEvent();

                e = (Event) e.Clone();
                e.printArea = printArea;

                eventDB.ChangeEvent(e);
            }
            else {
                Course course = eventDB.GetCourse(courseId);

                course = (Course) course.Clone();
                course.printArea = printArea;

                eventDB.ReplaceCourse(courseId, course);
            }
        }

        // Change the attributes of a course.
        public static void ChangeCourseProperties(EventDB eventDB, Id<Course> courseId, CourseKind courseKind, string courseName, string secondaryTitle, float printScale, float climb, DescriptionKind descriptionKind, int firstControlOrdinal)
        {
            Course course = eventDB.GetCourse(courseId);

            course = (Course) course.Clone();
            course.kind = courseKind;
            course.name = courseName;
            course.secondaryTitle = secondaryTitle;
            course.printScale = printScale;
            course.climb = climb;
            course.descKind = descriptionKind;
            course.firstControlOrdinal = firstControlOrdinal;

            eventDB.ReplaceCourse(courseId, course);
        }

        // Change the load of a course.
        public static void ChangeCourseLoad(EventDB eventDB, Id<Course> courseId, int newLoad)
        {
            Course course = eventDB.GetCourse(courseId);

            course = (Course) course.Clone();
            course.load = newLoad;

            eventDB.ReplaceCourse(courseId, course);
        }

        // Change the sort order of a course.
        public static void ChangeCourseSortOrder(EventDB eventDB, Id<Course> courseId, int newSortOrder)
        {
            if (newSortOrder <= 0)
                throw new ApplicationException("Bad sort order");

            Course course = eventDB.GetCourse(courseId);

            course = (Course) course.Clone();
            course.sortOrder = newSortOrder;

            eventDB.ReplaceCourse(courseId, course);
        }


        // temp struct used to maintain information about which legs need gaps to be updated.
        struct LegGapChange
        {
            public Id<ControlPoint> controlId1;
            public Id<ControlPoint> controlId2;
            public SymPath legPath;

            public LegGapChange(Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, SymPath legPath)
            {
                this.controlId1 = controlId1;
                this.controlId2 = controlId2;
                this.legPath = legPath;
            }
        }

        // Change the location of a control. Used when dragging a control to a new location, for example.
        public static void ChangeControlLocation(EventDB eventDB, Id<ControlPoint> controlId, PointF newLocation)
        {
            // Check to see if any legs exist that include this control, and if any of those legs have gaps.
            List<LegGapChange> legGapChangeList = new List<LegGapChange>();

            foreach (Id<Leg> legId in eventDB.AllLegIds) {
                Leg leg = eventDB.GetLeg(legId);
                if ((leg.controlId1 == controlId || leg.controlId2 == controlId) && leg.gaps != null)
                    legGapChangeList.Add(new LegGapChange(leg.controlId1, leg.controlId2, QueryEvent.GetLegPath(eventDB, leg.controlId1, leg.controlId2, legId)));
            }
            
            // Move the control.
            ControlPoint control = eventDB.GetControl(controlId);

            control = (ControlPoint) control.Clone();
            control.location = newLocation;

            eventDB.ReplaceControlPoint(controlId, control);

            // If there are any leg gaps that need to be repositioned, do that.
            if (legGapChangeList.Count > 0) {
                foreach (LegGapChange legGapChange in legGapChangeList) {
                    Id<Leg> legId = QueryEvent.FindLeg(eventDB, legGapChange.controlId1, legGapChange.controlId2);
                    if (legId.IsNotNone) {
                        Leg leg = (Leg) eventDB.GetLeg(legId).Clone();
                        SymPath newPath = QueryEvent.GetLegPath(eventDB, legGapChange.controlId1, legGapChange.controlId2, legId);
                        LegGap[] newGaps = LegGap.MoveGapsToNewPath(leg.gaps, legGapChange.legPath, newPath);
                        ChangeEvent.ChangeLegGaps(eventDB, legGapChange.controlId1, legGapChange.controlId2, newGaps);
                    }
                }
            }
        }

        // Change the orientation of a control. Must be a crossing point.
        public static void ChangeControlOrientation(EventDB eventDB, Id<ControlPoint> controlId, float newOrientation)
        {
            ControlPoint control = eventDB.GetControl(controlId);

            Debug.Assert(control.kind == ControlPointKind.CrossingPoint);

            control = (ControlPoint) control.Clone();
            control.orientation = newOrientation;

            eventDB.ReplaceControlPoint(controlId, control);
        }

        // Change the gaps of a control for a given scale.
        public static void ChangeControlGaps(EventDB eventDB, Id<ControlPoint> controlId, float scale, uint newGaps)
        {
            ControlPoint control = eventDB.GetControl(controlId);

            control = (ControlPoint) control.Clone();

            int scaleInt = (int) Math.Round(scale);     // scale is stored as int in the gaps to prevent rounding problems.

            if (newGaps == 0xFFFFFFFF) {
                if (control.gaps != null)
                    control.gaps.Remove(scaleInt);
            }
            else {
                if (control.gaps == null)
                    control.gaps = new Dictionary<int, uint>();
                control.gaps[scaleInt] = newGaps;
            }

            eventDB.ReplaceControlPoint(controlId, control); 
        }


        // Change the locations associated with a special.
        public static void ChangeSpecialLocations(EventDB eventDB, Id<Special> specialId, PointF[] newLocations)
        {
            Special special = eventDB.GetSpecial(specialId);

            special = (Special) special.Clone();
            special.locations = (PointF[]) newLocations.Clone();

            eventDB.ReplaceSpecial(specialId, special);
        }

        // Change the orientation associated with a special. Must be an optional crossing point.
        public static void ChangeSpecialOrientation(EventDB eventDB, Id<Special> specialId, float newOrientation)
        {
            Special special = eventDB.GetSpecial(specialId);

            Debug.Assert(special.kind == SpecialKind.OptCrossing);

            special = (Special) special.Clone();
            special.orientation = newOrientation;

            eventDB.ReplaceSpecial(specialId, special);
        }

        // Change the text associated with a special. Must be an text special
        public static void ChangeSpecialText(EventDB eventDB, Id<Special> specialId, string newText)
        {
            Special special = eventDB.GetSpecial(specialId);

            Debug.Assert(special.kind == SpecialKind.Text);

            special = (Special) special.Clone();
            special.text = newText;

            eventDB.ReplaceSpecial(specialId, special);
        }



        // Remove a control from a course. Caller must ensure the current is actually in this course.
        public static void RemoveCourseControl(EventDB eventDB, Id<Course> courseId, Id<CourseControl> courseControlIdRemove)
        {
            // UNDONE: This function does not correctly handle splits and joins yet.

            Id<CourseControl> currentId;   // current course control id
            Id<CourseControl> afterRemove = eventDB.GetCourseControl(courseControlIdRemove).nextCourseControl;

            Course course = eventDB.GetCourse(courseId);
            if (course.firstCourseControl == courseControlIdRemove) {
                // Special case -- remove the first course control.
                course = (Course) course.Clone();
                course.firstCourseControl = afterRemove;
                eventDB.ReplaceCourse(courseId, course);
                eventDB.RemoveCourseControl(courseControlIdRemove);
                return;
            }

            // Loop through all the course controls, remove one when we find it.
            currentId = course.firstCourseControl;
            while (currentId.IsNotNone) {
                CourseControl current = eventDB.GetCourseControl(currentId);
                if (current.nextCourseControl == courseControlIdRemove) {
                    current = (CourseControl) current.Clone();
                    current.nextCourseControl = afterRemove;
                    eventDB.ReplaceCourseControl(currentId, current);
                    eventDB.RemoveCourseControl(courseControlIdRemove);
                    return;
                }

                currentId = current.nextCourseControl;
            }

            throw new InvalidOperationException("Course Control not found");
        }

        // Removes a control from the event. If the control is present in any course as a course-control, those
        // course-controls are also removed.
        public static void RemoveControl(EventDB eventDB, Id<ControlPoint> controlId)
        {
            // Find all of the courses/course-controls that are this control.
            foreach (Id<Course> courseId in QueryEvent.CoursesUsingControl(eventDB, controlId)) {
                foreach (Id<CourseControl> courseControlId in QueryEvent.GetCourseControlsInCourse(eventDB, courseId, controlId)) {
                    RemoveCourseControl(eventDB, courseId, courseControlId);
                }
            }

            // Find all of the legs that use this control and remove them.
            List<Id<Leg>> legIdList = new List<Id<Leg>>();

            foreach (Id<Leg> legId in eventDB.AllLegIds) {
                Leg leg = eventDB.GetLeg(legId);
                if (leg.controlId1 == controlId || leg.controlId2 == controlId)
                    legIdList.Add(legId);
            }
            foreach (Id<Leg> legId in legIdList)
                eventDB.RemoveLeg(legId);

            // Remove the control point itself.
            eventDB.RemoveControlPoint(controlId);
        }

        // Add a new control point to the all controls collection. Doesn't add it to any courses, even for a start/finish control.
        public static Id<ControlPoint> AddControlPoint(EventDB eventDB, ControlPointKind kind, string code, PointF location, float orientation)
        {
            ControlPoint newControlPoint = new ControlPoint(kind, code, location);
            newControlPoint.orientation = orientation;
            return eventDB.AddControlPoint(newControlPoint);
        }

        // Add a new course control to a course. Adds a new CourseControl referencing controlId into courseId. The place to insert is
        // given by courseControl1 and courseControl2. These control should have been gotten by calling FindControlInsertionPoint.
        public static Id<CourseControl> AddCourseControl(EventDB eventDB, Id<ControlPoint> controlId, Id<Course> courseId, Id<CourseControl> courseControl1, Id<CourseControl> courseControl2)
        {
            CourseControl newCourseControl;
            Id<CourseControl> newCourseControlId;

            if (courseControl1.IsNone) {
                // Adding at start.
                Course course = (Course) eventDB.GetCourse(courseId).Clone();
                Debug.Assert(courseControl2 == course.firstCourseControl);
                newCourseControl = new CourseControl(controlId, course.firstCourseControl);
                newCourseControlId = eventDB.AddCourseControl(newCourseControl);
                course.firstCourseControl = newCourseControlId;
                eventDB.ReplaceCourse(courseId, course);
            }
            else {
                // Adding after courseControl1.
                CourseControl before = (CourseControl) eventDB.GetCourseControl(courseControl1).Clone();
                if (before.split) {
                    throw new NotImplementedException("Not yet implemented.");    // UNDONE: not yet implemented
                }
                else {
                    // Not a split.
                    Debug.Assert(courseControl2 == before.nextCourseControl);
                    newCourseControl = new CourseControl(controlId, before.nextCourseControl);
                    newCourseControlId = eventDB.AddCourseControl(newCourseControl);
                    before.nextCourseControl = newCourseControlId;
                    eventDB.ReplaceCourseControl(courseControl1, before);
                }
            }

            return newCourseControlId;
        }

        // Add a start control to a given course. If the course already has a start control as the first control, replace it.
        // Otherwise add it as the new start control. The returned CourseControl may be new, or may be the first control with 
        // a different control point.
        // If addToOtherCourses is true, the new start control is also added to all courses without an existing start control.
        public static Id<CourseControl> AddStartToCourse(EventDB eventDB, Id<ControlPoint> controlId, Id<Course> courseId, bool addToOtherCourses)
        {
            Course course = eventDB.GetCourse(courseId);
            Id<CourseControl> firstId = course.firstCourseControl;
            Id<CourseControl> newCourseControlId;
            if (firstId.IsNotNone && eventDB.GetControl(eventDB.GetCourseControl(firstId).control).kind == ControlPointKind.Start) {
                // First control exists and is a start control. Replace it.
                CourseControl first = (CourseControl) eventDB.GetCourseControl(firstId).Clone();
                first.control = controlId;
                eventDB.ReplaceCourseControl(firstId, first);
                newCourseControlId = firstId;
            }
            else {
                // Add the control as a new start control.
                newCourseControlId = eventDB.AddCourseControl(new CourseControl(controlId, firstId));
                course = (Course) course.Clone();
                course.firstCourseControl = newCourseControlId;
                eventDB.ReplaceCourse(courseId, course);
            }

            if (addToOtherCourses) {
                List<Id<Course>> courseModificationList = new List<Id<Course>>();

                // Check all courses to see if we should add the start to that course too.
                foreach (Id<Course> courseSearchId in eventDB.AllCourseIds) {
                    if (!QueryEvent.HasStartControl(eventDB, courseSearchId)) {
                        // This course does not have a start control. 
                        courseModificationList.Add(courseSearchId);
                    }
                }

                // Add the start control to each of those courses.
                foreach (Id<Course> modifyCourseId in courseModificationList)
                    AddStartToCourse(eventDB, controlId, modifyCourseId, false);
            }

            return newCourseControlId;
        }

        // Add a finish control to a given course. If the course already has a finish control as the last control, replace it.
        // Otherwise add it as the new finish control. The returned CourseControl may be new, or may be the last control with 
        // a different control point.
        // If addToOtherCourses is true, the new finish control is also added to all courses without an existing finish control.
        public static Id<CourseControl> AddFinishToCourse(EventDB eventDB, Id<ControlPoint> controlId, Id<Course> courseId, bool addToOtherCourses)
        {
            Course course = eventDB.GetCourse(courseId);
            Id<CourseControl> lastId = QueryEvent.LastCourseControl(eventDB, courseId, false); 
            Id<CourseControl> newCourseControlId;

            if (lastId.IsNone) {
                // No controls in the course. add this as the only control in the course.
                newCourseControlId = eventDB.AddCourseControl(new CourseControl(controlId, Id<CourseControl>.None));
                course = (Course) course.Clone();
                course.firstCourseControl = newCourseControlId;
                eventDB.ReplaceCourse(courseId, course);
            }
            else if (eventDB.GetControl(eventDB.GetCourseControl(lastId).control).kind == ControlPointKind.Finish) {
                // Last control exists and is a finish control. Replace it.
                CourseControl last = (CourseControl) eventDB.GetCourseControl(lastId).Clone();
                last.control = controlId;
                eventDB.ReplaceCourseControl(lastId, last);
                newCourseControlId = lastId;
            }
            else {
                // Last control exist but is not a finish. Add the finish onto it.
                newCourseControlId = eventDB.AddCourseControl(new CourseControl(controlId, Id<CourseControl>.None));
                CourseControl last = (CourseControl) eventDB.GetCourseControl(lastId).Clone();
                last.nextCourseControl = newCourseControlId;
                eventDB.ReplaceCourseControl(lastId, last);
            }

            if (addToOtherCourses) {
                List<Id<Course>> courseModificationList = new List<Id<Course>>();

                // Check all courses to see if we should add the finish to that course too.
                foreach (Id<Course> courseSearchId in eventDB.AllCourseIds) {
                    if (!QueryEvent.HasFinishControl(eventDB, courseSearchId)) {
                        // This course does not have a finish control. 
                        courseModificationList.Add(courseSearchId);
                    }
                }

                // Add the finish control to each of those courses.
                foreach (Id<Course> modifyCourseId in courseModificationList)
                    AddFinishToCourse(eventDB, controlId, modifyCourseId, false);
            }

            return newCourseControlId;
        }

        // Add a point special to the event. The special is visible in all courses.
        public static Id<Special> AddPointSpecial(EventDB eventDB, SpecialKind specialKind, PointF location, float orientation)
        {
            Special special = new Special(specialKind, new PointF[1] { location });
            special.orientation = orientation;
            return eventDB.AddSpecial(special);
        }

        // Add a line or area special to the event. The special is visible in all courses.
        public static Id<Special> AddLineAreaSpecial(EventDB eventDB, SpecialKind specialKind, PointF[] locations)
        {
            Special special = new Special(specialKind, locations);
            return eventDB.AddSpecial(special);
        }

        // Add a description to the event. 
        public static Id<Special> AddDescription(EventDB eventDB, bool allCourses, Id<Course>[] courses, PointF topLeft, float cellSize)
        {
            Special special = new Special(SpecialKind.Descriptions, new PointF[2] { topLeft, new PointF(topLeft.X + cellSize, topLeft.Y) });
            special.allCourses = allCourses;
            if (! allCourses)
                special.courses = courses;
            Id<Special> specialId =  eventDB.AddSpecial(special);

            // Descriptions special are unique per course -- enforce this.
            UpdateDescriptionCourses(eventDB, specialId);

            return specialId;
        }

        // Add a text special to the event
        public static Id<Special> AddTextSpecial(EventDB eventDB, RectangleF boundingRectangle, string text, string fontName, bool bold, bool italic)
        {
            Special special = new Special(SpecialKind.Text, new PointF[2] { new PointF(boundingRectangle.Left, boundingRectangle.Bottom), new PointF(boundingRectangle.Right, boundingRectangle.Top) });
            special.text = text;
            special.fontName = fontName;
            special.fontBold = bold;
            special.fontItalic = italic;

            return eventDB.AddSpecial(special);
        }

        // Delete a course and all of its course controls.
        public static void DeleteCourse(EventDB eventDB, Id<Course> courseId)
        {
            // Remember the set of course controls.
            List<Id<CourseControl>> courseControls = new List<Id<CourseControl>>(QueryEvent.EnumCourseControlIds(eventDB, courseId));

            // Remove the course.
            eventDB.RemoveCourse(courseId);

            // Remove each of the course controls in that course
            foreach (Id<CourseControl> courseControlId in courseControls) {
                eventDB.RemoveCourseControl(courseControlId);
            }

            // Now check specials, and see which need to be modified
            List<Id<Special>> specialsToChange = new List<Id<Special>>();
            foreach (Id<Special> specialId in eventDB.AllSpecialIds) {
                Special special = eventDB.GetSpecial(specialId);
                if (!special.allCourses && Array.IndexOf<Id<Course>>(special.courses, courseId) >= 0) {
                    // This special is not an all controls special, and is present on the course being deleted. Update it.
                    specialsToChange.Add(specialId);
                }
            }

            // Update each of the specials.
            foreach (Id<Special> specialId in specialsToChange) {
                Special special = eventDB.GetSpecial(specialId);
                if (special.courses.Length == 1)
                    ChangeEvent.DeleteSpecial(eventDB, specialId);
                else {
                    special = (Special) special.Clone();
                    List<Id<Course>> list = new List<Id<Course>>(special.courses);
                    list.Remove(courseId);
                    special.courses = list.ToArray();
                    eventDB.ReplaceSpecial(specialId, special);
                }
            }
        }

        // If exactly one control of the given kind exists in the event, return the ID of it, otherwise, return None..
        private static Id<ControlPoint> FindUniqueControl(EventDB eventDB, Id<Course> courseId, ControlPointKind controlPointKind)
        {
            Id<ControlPoint> uniqueControlId = Id<ControlPoint>.None;

            foreach (Id<ControlPoint> controlId in eventDB.AllControlPointIds) {
                if (eventDB.GetControl(controlId).kind == controlPointKind) {
                    if (uniqueControlId.IsNotNone)
                        return Id<ControlPoint>.None;             // already found one, so not unique.
                    else
                        uniqueControlId = controlId;
                }
            }

            return uniqueControlId;
        }

        // Create a new course with the given attributes. The course sorts after all existing courses.
        // If addStartAndFinish is true, then if exact one start control exists, it is added. If exactly one finish control exists, it is added.
        public static Id<Course> CreateCourse(EventDB eventDB, CourseKind courseKind, string name, string secondaryTitle, float printScale, float climb, DescriptionKind descriptionKind, int firstControlOrdinal, bool addStartAndFinish)
        {
            // Find max sort order in use.
            int maxSortOrder = 0;
            foreach (Course course in eventDB.AllCourses)
                if (course.sortOrder > maxSortOrder)
                    maxSortOrder = course.sortOrder;

            Course newCourse = new Course(courseKind, name, printScale, maxSortOrder + 1);
            newCourse.secondaryTitle = secondaryTitle;
            newCourse.climb = climb;
            newCourse.descKind = descriptionKind;
            newCourse.firstControlOrdinal = firstControlOrdinal;

            Id<Course> newCourseId = eventDB.AddCourse(newCourse);

            if (addStartAndFinish) {
                // Add unique start and finish, if they exist.
                Id<ControlPoint> uniqueStart = FindUniqueControl(eventDB, newCourseId, ControlPointKind.Start);
                if (uniqueStart.IsNotNone)
                    AddStartToCourse(eventDB, uniqueStart, newCourseId, false);

                Id<ControlPoint> uniqueFinish = FindUniqueControl(eventDB, newCourseId, ControlPointKind.Finish);
                if (uniqueFinish.IsNotNone)
                    AddFinishToCourse(eventDB, uniqueFinish, newCourseId, false);
            }

            return newCourseId;
        }

        // Delete a special
        public static void DeleteSpecial(EventDB eventDB, Id<Special> specialId)
        {
            eventDB.RemoveSpecial(specialId);
        }

        // Change the flagging associated with a leg. If changing to Begin/End flagging, then a bend will be introduced if no bends currently exist
        // in the leg. If the leg ends in the finish, the finish symbol may be changed to match if appropriate.
        public static void ChangeFlagging(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, FlaggingKind flagging)
        {
            ControlPoint control1 = eventDB.GetControl(controlId1);
            ControlPoint control2 = eventDB.GetControl(controlId2);

            if (control2.kind == ControlPointKind.Finish && flagging == FlaggingKind.All) {
                // If the leg ends in the finish control, we can set all flagging by just changing the finish control symbol.
                ChangeDescriptionSymbol(eventDB, controlId2, 0, "14.1");
                return;
            }

            // We need a leg object. Create a new one or get the existing one.
            Id<Leg> legId = QueryEvent.FindLeg(eventDB, controlId1, controlId2);
            Leg leg;
            if (legId.IsNone)
                leg = new Leg(controlId1, controlId2);
            else
                leg = (Leg) eventDB.GetLeg(legId).Clone();

            // Set the flagging kind.
            leg.flagging = flagging;

            if (flagging == FlaggingKind.Begin || flagging == FlaggingKind.End) {
                // These kinds of flagging require a bend in the flaggingStartStop field.
                if (leg.bends != null && leg.bends.Length > 0) {
                    // Already have a bend we can use.
                    leg.flagStartStop = (flagging == FlaggingKind.Begin) ? leg.bends[leg.bends.Length - 1] : leg.bends[0];
                }
                else {
                    // Create a bend half-way along the leg.
                    leg.flagStartStop = new PointF((control1.location.X + control2.location.X) / 2, (control1.location.Y + control2.location.Y) / 2);
                    leg.bends = new PointF[] { leg.flagStartStop };
                }
            }

            // Update the leg object.
            if (legId.IsNone)
                eventDB.AddLeg(leg);
            else {
                if (leg.IsVacuous())
                    eventDB.RemoveLeg(legId);
                else
                    eventDB.ReplaceLeg(legId, leg);
            }

            // Update the finish control symbol if reasonable.
            if (control2.kind == ControlPointKind.Finish) {
                // Update the finish control symbol.
                if ((flagging == FlaggingKind.None || flagging == FlaggingKind.Begin) && control2.symbolIds[0] == "14.1") {
                    // Remove the "flagged from last control symbol" and change it to "no flagging".
                    ChangeDescriptionSymbol(eventDB, controlId2, 0, "14.3");
                }
                else if (flagging == FlaggingKind.End) {
                    // If partial flagging on the end part of the leg, change the symbol to finish funnel.
                    ChangeDescriptionSymbol(eventDB, controlId2, 0, "14.2");
                }
            }

        }

        // Change the set of courses that a special is on. If all the courses are in the new array, changes the special to display in 
        // all courses.
        public static void ChangeDisplayedCourses(EventDB eventDB, Id<Special> specialId, Id<Course>[] displayedCourses)
        {
            // Check if the array contains all of the courses.
            bool allCourses = true;;

            foreach (Id<Course> courseId in eventDB.AllCourseIds) {
                if (Array.IndexOf(displayedCourses, courseId) < 0) {
                    allCourses = false;
                    break;
                }
            }

            Special special = (Special) eventDB.GetSpecial(specialId).Clone();
            special.allCourses = allCourses;
            if (allCourses) 
                special.courses = null;
            else
                special.courses = displayedCourses;
            eventDB.ReplaceSpecial(specialId, special);

            // Descriptions special are unique per course -- enforce this.
            if (special.kind == SpecialKind.Descriptions)
                UpdateDescriptionCourses(eventDB, specialId);
        }

        // Check all descriptions other than the passed one, and remove any duplicate courses.
        public static void UpdateDescriptionCourses(EventDB eventDB, Id<Special> descriptionId)
        {
            // Which courses to remove?
            Id<Course>[] coursesToRemove = QueryEvent.GetSpecialDisplayedCourses(eventDB, descriptionId);

            // Find all descriptions to change.
            List<Id<Special>> allDescriptionIds = new List<Id<Special>>();
            foreach (Id<Special> specialId in eventDB.AllSpecialIds) {
                if (eventDB.GetSpecial(specialId).kind == SpecialKind.Descriptions && specialId != descriptionId)
                    allDescriptionIds.Add(specialId);
            }

            foreach (Id<Special> descriptionToChange in allDescriptionIds) {
                // Remove any courses that overlap with the courses the given description has..
                bool changes = false;          // track if any changes made.
                List<Id<Course>> courses = new List<Id<Course>>(QueryEvent.GetSpecialDisplayedCourses(eventDB, descriptionToChange));
                foreach (Id<Course> courseId in coursesToRemove) {
                    if (courses.Contains(courseId)) {
                        changes = true;
                        courses.Remove(courseId);
                    }
                }

                // Commit the removal to the event database.
                if (changes) {
                    if (courses.Count == 0) {
                        // Remove the given description entire, since it has no displayed courses.
                        eventDB.RemoveSpecial(descriptionToChange);
                    }
                    else {
                        Special newDescription = (Special) eventDB.GetSpecial(descriptionToChange).Clone();
                        newDescription.allCourses = false;
                        newDescription.courses = courses.ToArray();
                        eventDB.ReplaceSpecial(descriptionToChange, newDescription);
                    }
                }
            }
        }

        // Move the bend in a leg to a new location.
        public static void MoveLegBend(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, PointF oldBend, PointF newBend)
        {
            // Get the old leg.
            Id<Leg> legId = QueryEvent.FindLeg(eventDB, controlId1, controlId2);
            Debug.Assert(legId.IsNotNone);
            Leg leg = (Leg) eventDB.GetLeg(legId).Clone();
            SymPath oldPath = QueryEvent.GetLegPath(eventDB, controlId1, controlId2, legId);

            // Change the bend.
            for (int i = 0; i < leg.bends.Length; ++i) {
                if (leg.bends[i] == oldBend) 
                    leg.bends[i] = newBend;
            }
            if (leg.flagStartStop == oldBend)
                leg.flagStartStop = newBend;

            // Update the leg.
            eventDB.ReplaceLeg(legId, leg);

            // If the leg had gaps, update the gaps for the new path.
            if (leg.gaps != null) {
                SymPath newPath = QueryEvent.GetLegPath(eventDB, controlId1, controlId2);
                leg.gaps = LegGap.MoveGapsToNewPath(leg.gaps, oldPath, newPath);
                eventDB.ReplaceLeg(legId, leg);
            }
        }

        // Add a bend to a leg. 
        public static void AddLegBend(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, PointF newBend)
        {
            // Get the leg object for this leg. Create one if needed.
            Id<Leg> legId = QueryEvent.FindLeg(eventDB, controlId1, controlId2);
            Leg leg;
            if (legId.IsNone)
                leg = new Leg(controlId1, controlId2);
            else
                leg = (Leg) eventDB.GetLeg(legId).Clone();
            SymPath oldPath = QueryEvent.GetLegPath(eventDB, controlId1, controlId2, legId);

            // Get an array with the start/end points and the bends.
            PointF[] oldBendArray = new PointF[(leg.bends == null) ? 2 : leg.bends.Length + 2];
            if (leg.bends != null)
                Array.Copy(leg.bends, 0, oldBendArray, 1, leg.bends.Length);
            oldBendArray[0] = eventDB.GetControl(controlId1).location;
            oldBendArray[oldBendArray.Length - 1] = eventDB.GetControl(controlId2).location;

            // Insert the new point into the array at the right place.
            PointF[] newBendArray = Util.AddPointToArray(oldBendArray, newBend);

            // Copy the new bend parts into the bends array.
            leg.bends = new PointF[newBendArray.Length - 2];
            Array.Copy(newBendArray, 1, leg.bends, 0, newBendArray.Length - 2);

            // Update the leg.
            if (legId.IsNone)
                eventDB.AddLeg(leg);
            else
                eventDB.ReplaceLeg(legId, leg);

            // If the leg had gaps, update the gaps for the new path.
            if (leg.gaps != null) {
                SymPath newPath = QueryEvent.GetLegPath(eventDB, controlId1, controlId2);
                LegGap[] newGaps = LegGap.MoveGapsToNewPath(leg.gaps, oldPath, newPath);
                ChangeLegGaps(eventDB, controlId1, controlId2, newGaps);
            }
        }

        // Remove a bend from a leg.
        public static void RemoveLegBend(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, PointF bendToRemove)
        {
            bool newFlagging = false;
            FlaggingKind newFlaggingKind = FlaggingKind.None;

            // Get the leg object for this leg. One must exists
            Id<Leg> legId = QueryEvent.FindLeg(eventDB, controlId1, controlId2);
            Leg leg = (Leg) eventDB.GetLeg(legId).Clone();
            SymPath oldPath = QueryEvent.GetLegPath(eventDB, controlId1, controlId2, legId);

            if (leg.flagging == FlaggingKind.Begin || leg.flagging == FlaggingKind.End && leg.flagStartStop == bendToRemove) {
                // We are removing the point at which flagging starts/stop. The start/stop point must move to another, unless there are no 
                // other bends left.
                if (leg.bends.Length == 1) {
                    // No other bends left. Make leg all flagging.
                    newFlagging = true;
                    newFlaggingKind = FlaggingKind.All;
                }
                else {
                    // Basic idea is to move to the bend that is at the flagging end, unless there is no such bend.

                    // Where is the bend?
                    int index = Array.IndexOf(leg.bends, bendToRemove);

                    if ((index == 0 || leg.flagging == FlaggingKind.End) && index != leg.bends.Length - 1) {
                        // move to next bend after the one removed.
                        leg.flagStartStop = leg.bends[index + 1];
                    }
                    else {
                        // move to previous bend before the one removed.
                        leg.flagStartStop = leg.bends[index - 1];
                    }
                }
            }

            // Remove the bend from the bend array.
            leg.bends = Util.RemovePointFromArray(leg.bends, bendToRemove);
            if (leg.bends.Length == 0)
                leg.bends = null;

            // Update the leg objects.
            if (leg.IsVacuous())
                eventDB.RemoveLeg(legId); 
            else
                eventDB.ReplaceLeg(legId, leg);

            // Change flagging if we need to. This is more complex that just setting the flagging kind in the leg object.
            if (newFlagging)
                ChangeFlagging(eventDB, controlId1, controlId2, newFlaggingKind);

            // If the leg had gaps, update the gaps for the new path.
            if (leg.gaps != null) {
                SymPath newPath = QueryEvent.GetLegPath(eventDB, controlId1, controlId2);
                LegGap[] newGaps = LegGap.MoveGapsToNewPath(leg.gaps, oldPath, newPath);
                ChangeLegGaps(eventDB, controlId1, controlId2, newGaps);
            }
        }

        // Change the gaps associated with a leg.
        public static void ChangeLegGaps(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, LegGap[] newGaps)
        {
            // Get the leg object for this leg. Create one if needed.
            Id<Leg> legId = QueryEvent.FindLeg(eventDB, controlId1, controlId2);
            Leg leg;
            if (legId.IsNone)
                leg = new Leg(controlId1, controlId2);
            else
                leg = (Leg) eventDB.GetLeg(legId).Clone();

            // Change the gaps.
            leg.gaps = (newGaps == null) ? null : (LegGap[]) newGaps.Clone();

            // Write the change leg object to the event DB. If the leg is vacuous, could involve removing the leg.
            if (leg.IsVacuous()) {
                if (legId.IsNotNone)
                    eventDB.RemoveLeg(legId);
            }
            else {
                if (legId.IsNone)
                    eventDB.AddLeg(leg);
                else
                    eventDB.ReplaceLeg(legId, leg);
            }
        }

        // Move a gap start/end point
        public static void MoveLegGap(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, PointF oldPoint, PointF newPoint)
        {
            SymPath path = QueryEvent.GetLegPath(eventDB, controlId1, controlId2);
            LegGap[] gaps = QueryEvent.GetLegGaps(eventDB, controlId1, controlId2);
            ChangeLegGaps(eventDB, controlId1, controlId2, LegGap.SimplifyGaps(LegGap.MoveStartStopPoint(path, gaps, oldPoint, newPoint), path.Length));
        }

        // Remove the leg gap that a given point lies in/close it.
        public static void RemoveLegGap(EventDB eventDB, Id<ControlPoint> controlId1, Id<ControlPoint> controlId2, PointF gapLocation)
        {
            SymPath path = QueryEvent.GetLegPath(eventDB, controlId1, controlId2);
            LegGap[] gaps = QueryEvent.GetLegGaps(eventDB, controlId1, controlId2);

            // Where, in distance along the path, does the gapLocation lie?
            float distanceToGapLocation = path.LengthToPoint(gapLocation);

            // Go through the gaps, if this location is within a gap, remove it.
            bool gapFound = false;
            List<LegGap> list = new List<LegGap>(gaps);
            for (int i = 0; i < list.Count; ++i) {
                if (list[i].distanceFromStart <= distanceToGapLocation && list[i].distanceFromStart + list[i].length >= distanceToGapLocation) {
                    list.RemoveAt(i);
                    gapFound = true;
                    break;
                }
            }

            // If we found and removed a gap, then commit that change to the leg.
            if (gapFound) 
                ChangeLegGaps(eventDB, controlId1, controlId2, LegGap.SimplifyGaps(list.ToArray(), path.Length));
        }


        // Add a corner to a special. 
        public static void AddSpecialCorner(EventDB eventDB, Id<Special> specialId, PointF newCorner)
        {
            Special special = eventDB.GetSpecial(specialId);
            Debug.Assert(special.kind == SpecialKind.OOB || special.kind == SpecialKind.Dangerous || special.kind == SpecialKind.Boundary || special.kind == SpecialKind.WhiteOut);
            bool isArea = (special.kind != SpecialKind.Boundary);

            PointF[] oldLocations, newLocations;

            // If it's an area special, add the first location as the last also.
            if (isArea) {
                oldLocations = new PointF[special.locations.Length + 1];
                Array.Copy(special.locations, oldLocations, special.locations.Length);
                oldLocations[special.locations.Length] = oldLocations[0];
            }
            else {
                oldLocations = special.locations;
            }

            // Add the corner in the right place.
            PointF[] newPoints = Util.AddPointToArray(oldLocations, newCorner);

            // If it's an area special, remove the first location from the end.
            if (isArea) {
                newLocations = new PointF[newPoints.Length - 1];
                Array.Copy(newPoints, newLocations, newPoints.Length - 1);
            }
            else {
                newLocations = newPoints;
            }

            // Update the special.
            ChangeSpecialLocations(eventDB, specialId, newLocations);
        }

        // Remove a corner from a special. 
        public static void RemoveSpecialCorner(EventDB eventDB, Id<Special> specialId, PointF cornerToRemove)
        {
            Special special = eventDB.GetSpecial(specialId);
            Debug.Assert(((special.kind == SpecialKind.OOB || special.kind == SpecialKind.Dangerous || special.kind == SpecialKind.WhiteOut) && special.locations.Length > 3) || 
                (special.kind == SpecialKind.Boundary && special.locations.Length > 2));

            // Remove the corner
            PointF[] newPoints = Util.RemovePointFromArray(special.locations, cornerToRemove);

            // Update the special.
            ChangeSpecialLocations(eventDB, specialId, newPoints);
        }

        // Add a gap at a particular radians to a gap bitflag field.
        public static uint AddGap(uint start, double radians)
        {
            int bitNumber = (int) Math.Round(radians / (2 * Math.PI) * 32.0);

            // Set 3 bits around the location of the gap.
            uint result = Util.SetBit(start, bitNumber - 1, false);
            result = Util.SetBit(result, bitNumber, false);
            result = Util.SetBit(result, bitNumber + 1, false);
            return result;
        }

        // Remove a gap at a particular radians  
        public static uint RemoveGap(uint start, double radians)
        {
            uint result = start;
            int bitNumber = (int) Math.Round(radians / (2 * Math.PI) * 32.0);

            // If there is no gap at the given bit number, do nothing.
            if (Util.GetBit(start, bitNumber) == true)
                return start;

            // Remove gap going up
            int i = bitNumber + 1;
            while (Util.GetBit(result, i) == false) {
                result = Util.SetBit(result, i, true);
                ++i;
            }

            // Remove map going down.
            i = bitNumber;
            while (Util.GetBit(result, i) == false) {
                result = Util.SetBit(result, i, true);
                --i;
            }

            return result;
        }

        // Set all punch patterns, by code, for the event.
        public static void SetAllPunchPatterns(EventDB eventDB, Dictionary<string, PunchPattern> allPatterns)
        {
            foreach (KeyValuePair<string, PunchPattern> pair in allPatterns) {
                Id<ControlPoint> controlId = QueryEvent.FindCode(eventDB, pair.Key);
                if (controlId.IsNotNone) {
                    ControlPoint controlPoint = (ControlPoint) eventDB.GetControl(controlId).Clone();
                    if (pair.Value == null)
                        controlPoint.punches = null;
                    else
                        controlPoint.punches = (PunchPattern) pair.Value.Clone();

                    eventDB.ReplaceControlPoint(controlId, controlPoint);
                }
            }
        }

        // Set the punch card format for the event.
        public static void ChangePunchcardFormat(EventDB eventDB, PunchcardFormat punchcardFormat)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.punchcardFormat = (PunchcardFormat) punchcardFormat.Clone();

            eventDB.ChangeEvent(e);
        }

        // Set the course appearance for this event.
        public static void ChangeCourseAppearance(EventDB eventDB, CourseAppearance courseAppearance)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.courseAppearance = (CourseAppearance) courseAppearance.Clone();

            eventDB.ChangeEvent(e);
        }

        // Change the custom system text for the event.
        public static void ChangeCustomSymbolText(EventDB eventDB, Dictionary<string, List<SymbolText>> customSymbolText, Dictionary<string, bool> customSymbolKey)
        {
            Event e = eventDB.GetEvent();

            e = (Event) e.Clone();
            e.customSymbolText = Util.CopyDictionary(customSymbolText);
            e.customSymbolKey = Util.CopyDictionary(customSymbolKey);

            eventDB.ChangeEvent(e); 
        }
    }

}
