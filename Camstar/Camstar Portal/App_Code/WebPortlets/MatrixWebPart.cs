// Copyright Siemens 2023  
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Camstar.WebPortal.Personalization;
using Camstar.WebPortal.PortalFramework;
using CamstarPortal.WebControls.Accordion;
using Camstar.WebPortal.FormsFramework.WebControls;
using System.Collections.Generic;
using Helpers;

namespace Camstar.WebPortal.WebPortlets
{
/// <summary>
/// Summary description for MatrixWebPart
/// </summary>
    [DynamicLayoutPersonalization]
    public class MatrixWebPart : MatrixWebPartBase
    {
        protected override IMatrixBuilder MatrixBuilder
        {
            get
            {
                if (_matrixBuilder == null)
                    _matrixBuilder = IsResponsive ? new DivLayoutBuilder() as IMatrixBuilder : new TableLayoutBuilder() as IMatrixBuilder;
                return  _matrixBuilder;
            }
        }

        protected override void InsertMatrixTable(ControlCollection contentControls)
        {
            if (Items.Count < 1)
                return;

            var cont = Model.PublishedContent as WebPartDefinition;
            var simplifiedAcc = false;
            if (cont != null && cont.DataContract != null && cont.DataContract.DataMembers != null)
            {
                var accMode = cont.DataContract.DataMembers.FirstOrDefault(d => d.Name == "AccordionControlMode");
                if (accMode != null && accMode.Key == "extended")
                {
                    simplifiedAcc = true;
                    _matrixBuilder = new DivLayoutBuilder() as IMatrixBuilder;
                }
            }

            CollapsibleGroupType[] collapsedGroups = null;
            if (Content != null)
                collapsedGroups = Content.CollapsibleGroups;
            if (collapsedGroups != null && collapsedGroups.Length > 0)
            {
                var mtrx = BuildMatrixTable();
                var rowsCount = 0;
                List<TableRow> tableRows = null;
                List<DivBlock> divRows = null;
                if (mtrx is Table)
                {
                    tableRows = (mtrx as Table).Rows.OfType<TableRow>().ToList();
                    rowsCount = tableRows.Count;
                }
                else
                {
                    divRows = (mtrx as WebControl).Controls.Cast<DivBlock>().Where(c => c.CssClass == "row").ToList();
                    rowsCount = divRows.Count;
                }

                if (mtrx != null && rowsCount > 0 && Content != null)
                {
                    var acc = new Accordion { AllowExpandAll = true, AutoSize = AutoSize.None, DisplayExpandAllButton = true, ID = "CollapsibleSectionsAccordion"};
                    if (simplifiedAcc)
                        acc.IsExtendedDesign = true;

                    int addedRows = 0;
                    int i = 0;
                    var isTable = mtrx is Table;
                    if (simplifiedAcc)
                        isTable = false;
                    foreach (var group in collapsedGroups.Where(group => group.Rows > 0))
                    {
                        if (rowsCount >= addedRows + group.Rows)
                        {
                            var pane = new AccordionPane(isTable)
                            {
                                HeaderLabelName = group.LabelName,
                                HeaderLabelText = group.LabelText
                            };
                            if (mtrx is Table)
                                pane.AddRows( tableRows.Skip(addedRows).Take(addedRows + group.Rows ?? 0).ToArray());
                            else
                                pane.AddRows(divRows.Skip(addedRows).Take(addedRows + group.Rows ?? 0).ToArray());

                            acc.Panes.Add(pane);
                            if (i == 0)
                            {
                                if (group.InitialState != InitialGroupStateType.Collapsed)  // expand the 1st group by default.
                                    acc.InitialExpandedSections.Add(i);
                            }
                            else
                            {
                                if (group.InitialState == InitialGroupStateType.Expanded) // collapse all the others sections by default.
                                    acc.InitialExpandedSections.Add(i);
                            }
                            addedRows += group.Rows ?? 0;
                            i++;
                        }
                        else
                            break;
                    }
                    contentControls.Add(acc);
                }
            }
            else
                base.InsertMatrixTable(contentControls);
        }

        IMatrixBuilder _matrixBuilder = null;
    }
}
