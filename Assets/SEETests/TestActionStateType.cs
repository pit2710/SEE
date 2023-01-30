﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SEE.Controls.Actions;

namespace SEETests
{
    /// <summary>
    /// Tests for the <see cref="ActionStateType"/> class.
    /// </summary>
    internal class TestActionStateType
    {
        private IList<ActionStateType> allTypes;

        [SetUp]
        public void SetUp()
        {
            allTypes = ActionStateType.AllTypes;
        }

        [Test]
        public void AllTypesJustContainsAllTypes()
        {
            IList<ActionStateType> actualTypes =
                typeof(ActionStateType).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.PropertyType == typeof(ActionStateType))
                                       .Select(x => (ActionStateType) x.GetValue(null)).ToList();
            Assert.AreEqual(actualTypes, allTypes, "ActionStateType.AllTypes must contain all of its types "
                                                   + "(and only those)!");
        }

        [Test]
        public void TestNoAttributeNull()
        {
            Assert.IsEmpty(allTypes.Where(x => x.Description == null || x.Name == null || x.IconPath == null),
                "No attribute of an ActionStateType may be null!");
        }

        [Test]
        public void TestNameUnique()
        {
            Assert.AreEqual(allTypes.Count, allTypes.Select(x => x.Name).Distinct().Count(),
                "Names of ActionStateType must be unique!");
        }

        public static IEnumerable<TestCaseData> AllTypeSupplier()
        {
            return ActionStateType.AllTypes.Select(type => new TestCaseData(type));
        }

        [Test, TestCaseSource(nameof(AllTypeSupplier))]
        public void TestEquality(ActionStateType type)
        {
            Assert.IsTrue(type.Equals(type));
            Assert.IsFalse(type.Equals(null));
            Assert.AreEqual(1, allTypes.Where(type.Equals).Count(),
                "An ActionStateType must only be equal to itself!");
        }
    }
}