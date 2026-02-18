using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Marks a class as containing test methods for the Teak test runner.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TeakTestFixtureAttribute : Attribute { }

/// <summary>
/// Marks a method as a test case. Must be a public instance method with no parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TeakTestAttribute : Attribute { }

/// <summary>
/// Marks a method to run once before any tests in the fixture.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TeakSetUpAttribute : Attribute { }

/// <summary>
/// Assertion helpers for Teak tests.
/// </summary>
public static class TeakAssert {
    public class AssertionFailedException : Exception {
        public AssertionFailedException(string message) : base(message) { }
    }

    public static void IsTrue(bool condition, string message = null) {
        if (!condition) {
            throw new AssertionFailedException(message ?? "Expected true but was false");
        }
    }

    public static void IsFalse(bool condition, string message = null) {
        if (condition) {
            throw new AssertionFailedException(message ?? "Expected false but was true");
        }
    }

    public static void AreEqual(object expected, object actual, string message = null) {
        if (!object.Equals(expected, actual)) {
            string detail = string.Format("Expected: {0}, Actual: {1}",
                expected ?? "(null)", actual ?? "(null)");
            throw new AssertionFailedException(message != null ? message + " — " + detail : detail);
        }
    }

    public static void IsNull(object value, string message = null) {
        if (value != null) {
            string detail = string.Format("Expected null but was: {0}", value);
            throw new AssertionFailedException(message != null ? message + " — " + detail : detail);
        }
    }

    public static void IsNotNull(object value, string message = null) {
        if (value == null) {
            throw new AssertionFailedException(message ?? "Expected non-null but was null");
        }
    }
}

/// <summary>
/// Discovers and runs all [TeakTestFixture] classes with [TeakTest] methods.
/// Invoke from the command line with: Unity -executeMethod TeakTestRunner.RunAll
/// </summary>
public static class TeakTestRunner {
    const string PREFIX = "[TeakTest]";

    public static void RunAll() {
        int totalPassed = 0;
        int totalFailed = 0;
        var failures = new List<string>();

        Debug.Log(PREFIX + " ---- Test Run Starting ----");

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            Type[] types;
            try {
                types = assembly.GetTypes();
            } catch (ReflectionTypeLoadException) {
                continue;
            }

            foreach (var type in types) {
                if (type.GetCustomAttribute<TeakTestFixtureAttribute>() == null) continue;

                string fixtureName = type.Name;
                Debug.Log(string.Format("{0} {1}", PREFIX, fixtureName));

                object instance;
                try {
                    instance = Activator.CreateInstance(type);
                } catch (Exception e) {
                    Debug.LogError(string.Format("{0}   FAIL (could not instantiate: {1})", PREFIX, e.Message));
                    totalFailed++;
                    continue;
                }

                // Collect test methods once
                var testMethods = new List<MethodInfo>();
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                    if (method.GetCustomAttribute<TeakTestAttribute>() != null) {
                        testMethods.Add(method);
                    }
                }

                // Run [TeakSetUp] if present
                bool setupFailed = false;
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                    if (method.GetCustomAttribute<TeakSetUpAttribute>() != null) {
                        try {
                            method.Invoke(instance, null);
                        } catch (Exception e) {
                            setupFailed = true;
                            var inner = e.InnerException ?? e;
                            Debug.LogError(string.Format("{0}   FAIL SetUp: {1}", PREFIX, inner.Message));
                            failures.Add(fixtureName + ".SetUp: " + inner.Message);
                        }
                    }
                }

                // If SetUp failed, skip all tests — running them would produce
                // confusing cascading failures.
                if (setupFailed) {
                    foreach (var method in testMethods) {
                        totalFailed++;
                        Debug.LogError(string.Format("{0}   SKIP {1} (SetUp failed)", PREFIX, method.Name));
                        failures.Add(fixtureName + "." + method.Name + ": skipped (SetUp failed)");
                    }
                    continue;
                }

                // Run tests
                foreach (var method in testMethods) {
                    string testName = fixtureName + "." + method.Name;
                    try {
                        method.Invoke(instance, null);
                        totalPassed++;
                        Debug.Log(string.Format("{0}   PASS {1}", PREFIX, method.Name));
                    } catch (Exception e) {
                        totalFailed++;
                        var inner = e.InnerException ?? e;
                        string failMsg = string.Format("{0}   FAIL {1}: {2}", PREFIX, method.Name, inner.Message);
                        Debug.LogError(failMsg);
                        failures.Add(testName + ": " + inner.Message);
                    }
                }
            }
        }

        Debug.Log(string.Format("{0} ---- Results: {1} passed, {2} failed ----",
            PREFIX, totalPassed, totalFailed));

        if (failures.Count > 0) {
            Debug.LogError(PREFIX + " Failures:");
            foreach (var f in failures) {
                Debug.LogError(PREFIX + "   " + f);
            }
        }

        EditorApplication.Exit(totalFailed > 0 ? 1 : 0);
    }
}
