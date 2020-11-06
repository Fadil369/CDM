﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.CommonDataModel.ObjectModel.Tests.Cdm
{
    using Microsoft.CommonDataModel.ObjectModel.Cdm;
    using Microsoft.CommonDataModel.ObjectModel.Enums;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Multiple test classes in projections test the attribute context tree generated for various scenarios.
    /// This utility class helps generate the actual attribute context generated by the scenario, so that it can be compared with expected attribute context tree.
    /// This also handles the validation of the expected vs. actual attribute context.
    /// </summary>
    public sealed class AttributeContextUtil
    {
        private StringBuilder bldr = new StringBuilder();

        /// <summary>
        /// Function to get the attribute context string tree from a resolved entity
        /// </summary>
        /// <param name="resolvedEntity"></param>
        /// <param name="attribContext"></param>
        /// <returns></returns>
        public string GetAttributeContextStrings(CdmEntityDefinition resolvedEntity, CdmAttributeContext attribContext)
        {
            // clear the string builder
            bldr.Clear();

            // get the corpus path for each attribute context in the tree
            GetContentDeclaredPath(resolvedEntity.AttributeContext);

            // get the traits for all the attributes of a resolved entity
            GetTraits(resolvedEntity);

            return bldr.ToString();
        }

        /// <summary>
        /// Get the corpus path for each attribute context in the tree and build a string collection that we can
        /// compare with the expected attribute context corpus path collection.
        /// </summary>
        /// <param name="attribContext"></param>
        private void GetContentDeclaredPath(CdmAttributeContext attribContext)
        {
            if (attribContext != null &&
                attribContext.Contents != null &&
                attribContext.Contents.Count > 0)
            {
                for (int i = 0; i < attribContext.Contents.Count; i++)
                {
                    string str = "";
                    if ((attribContext.Contents[0] is CdmAttributeReference))
                    {
                        CdmAttributeReference ar = (CdmAttributeReference)attribContext.Contents[i];
                        str = ar.AtCorpusPath;
                        bldr.AppendLine(str);

                    }
                    else
                    {
                        CdmAttributeContext ac = (CdmAttributeContext)attribContext.Contents[i];
                        str = ac.AtCorpusPath;
                        bldr.AppendLine(str);
                        GetContentDeclaredPath(ac);
                    }
                }
            }
        }

        /// <summary>
        /// Get the traits for all the attributes of a resolved entity
        /// </summary>
        /// <param name="resolvedEntity"></param>
        private void GetTraits(CdmEntityDefinition resolvedEntity)
        {
            foreach (CdmAttributeItem attrib in resolvedEntity.Attributes)
            {
                string attribCorpusPath = attrib.AtCorpusPath;
                bldr.AppendLine(attribCorpusPath);

                foreach (CdmTraitReference trait in attrib.AppliedTraits)
                {
                    string attribTraits = trait.NamedReference;
                    bldr.AppendLine(attribTraits);

                    foreach (CdmArgumentDefinition args in trait.Arguments)
                    {
                        GetArgumentValuesAsString(args);
                    }
                }
            }
        }

        private void GetArgumentValuesAsString(CdmArgumentDefinition args)
        {
            string paramName = args.ResolvedParameter?.Name;
            string paramDefaultValue = args.ResolvedParameter?.DefaultValue;

            if (!string.IsNullOrWhiteSpace(paramName) || !string.IsNullOrWhiteSpace(paramDefaultValue))
            {
                bldr.AppendLine($"  [Parameter (Name / DefaultValue): {paramName} / {paramDefaultValue}]");
            }

            if (args.Value is string)
            {
                string argsValue = args.Value;

                if (!string.IsNullOrWhiteSpace(argsValue))
                {
                    bldr.AppendLine($"  [Argument Value: {argsValue}]");
                }
            }
            else if (args.Value?.SimpleNamedReference == true)
            {
                string argsValue = args.Value.NamedReference;

                if (!string.IsNullOrWhiteSpace(argsValue))
                {
                    bldr.AppendLine($"  [Argument Value: {argsValue}]");
                }
            }
            else if (args.Value?.ExplicitReference.ObjectType == CdmObjectType.ConstantEntityDef)
            {
                var constEnt = (CdmConstantEntityDefinition)((CdmObjectReferenceBase)args.Value).ExplicitReference;
                if (constEnt != null)
                {
                    List<CdmEntityDefinition> refs = new List<CdmEntityDefinition>();
                    foreach (List<string> val in constEnt.ConstantValues)
                    {
                        bldr.AppendLine($"  [Argument Value: {string.Join(',', val)}]");
                    }
                }
            }
        }

        /// <summary>
        /// A function to validate if the attribute context tree & traits generated for a resolved entity is the same as the expected and saved attribute context tree & traits for a test case
        /// </summary>
        /// <param name="corpus"></param>
        /// <param name="expectedOutputPath"></param>
        /// <param name="entityName"></param>
        /// <param name="resolvedEntity"></param>
        public static void ValidateAttributeContext(CdmCorpusDefinition corpus, string expectedOutputPath, string entityName, CdmEntityDefinition resolvedEntity)
        {
            if (resolvedEntity.AttributeContext != null)
            {
                AttributeContextUtil attrCtxUtil = new AttributeContextUtil();

                // Expected
                string expectedStringFilePath = Path.GetFullPath(Path.Combine(expectedOutputPath, $"AttrCtx_{entityName}.txt"));
                string expectedText = File.ReadAllText(expectedStringFilePath);

                // Actual
                string actualStringFilePath = Path.GetFullPath(Path.Combine(expectedOutputPath, "..", "ActualOutput", $"AttrCtx_{entityName}.txt"));

                // Save Actual AttrCtx_*.txt and Resolved_*.cdm.json
                string actualText = attrCtxUtil.GetAttributeContextStrings(resolvedEntity, resolvedEntity.AttributeContext);
                File.WriteAllText(actualStringFilePath, actualText);
                resolvedEntity.InDocument.SaveAsAsync($"Resolved_{entityName}.cdm.json", saveReferenced: false).GetAwaiter().GetResult();

                // Test if Actual is Equal to Expected
                Assert.AreEqual(expectedText.Replace("\r\n", "\n"), actualText.Replace("\r\n", "\n"));
            }
        }
    }
}
