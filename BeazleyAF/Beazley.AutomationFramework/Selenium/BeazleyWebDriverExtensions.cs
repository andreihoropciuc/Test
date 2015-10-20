using System;
using Beazley.AutomationFramework.Contexts;
using Beazley.AutomationFramework.InfrastructureObjects;
using log4net;
using OpenQA.Selenium;

namespace Beazley.AutomationFramework.Selenium
{
    public static class BeazleyWebDriverExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BeazleyWebDriverExtensions));
		
		internal static ITimeouts ChangeImplicitTimeout(this IWebDriver driver, TimeSpan timeToWait)
		{
			return Globals.ChangeImplicitWait(timeToWait);
		}

		internal static ITimeouts RestoreImplicitTimeout(this IWebDriver driver)
		{
			return Globals.RestoreImplicitWait();
		}

		internal static IWebElement PeekElement(this IWebDriver driver, WebElement webElement, int seconds = 1)
		{
            using (ImplicitWaitContext.Create(TimeSpan.FromSeconds(seconds)))
            {
                return webElement.Element;
            }
		}

		private static void LogElemNotFound()
		{
			Log.Info("<element not found> Check if id mapping is correct.");
		}

		public static Func<IWebDriver, bool> ExpectedConditions_ElementIsVisible(WebElement webElement)
		{
			return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_ElementIsVisible",
									   (seleniumDriver, elementLocator, actionName) =>
										   {
												IWebElement targetElement = driver.PeekElement(webElement);

												if (targetElement == null)
												{
													LogElemNotFound();
													return false;
												}
												bool result;
												using (ImplicitWaitContext.Create(TimeSpan.FromSeconds(1)))
												{
                                                    result = targetElement.Displayed;
												}
												Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, result));

												return result;
										   });
		}
		public static Func<IWebDriver, bool> ExpectedConditions_ElementIsNotVisible(WebElement webElement)
		{
			return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_ElementIsNotVisible",
									   (seleniumDriver, elementLocator, actionName) =>
										   {
												IWebElement targetElement = driver.PeekElement(webElement);

												if (targetElement == null)
												{
													LogElemNotFound();
													return false;
												}
													
												bool result = !targetElement.Displayed;
												Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, result));

												return result;
										   });
		}

		public static Func<IWebDriver, bool> ExpectedConditions_ElementExists(WebElement webElement)
		{
			return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_ElementExists",
									   (seleniumDriver, elementLocator, actionName) =>
										   {
												IWebElement targetElement = driver.PeekElement(webElement);

												var elementWasFound = targetElement != null;

												if (!elementWasFound)
												{
													LogElemNotFound();
												}

												Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, elementWasFound));

												return elementWasFound;
										   });
		}
		public static Func<IWebDriver, bool> ExpectedConditions_ElementNotExists(WebElement webElement)
		{
			return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_ElementNotExists",
									   (seleniumDriver, elementLocator, actionName) =>
										   {
												IWebElement targetElement = driver.PeekElement(webElement);

												var elementWasNotFound = targetElement == null;

												if (elementWasNotFound)
												{
													LogElemNotFound();
												}

												Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, elementWasNotFound));

												return elementWasNotFound;
										   });
		}

		public static Func<IWebDriver, bool> ExpectedConditions_IsNotStale(WebElement webElement)
		{
			return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_IsNotStale",
									   (seleniumDriver, elementLocator, actionName) =>
										   {
											   IWebElement targetElement = driver.PeekElement(webElement);

											   var elementExists = targetElement != null;

											   Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, elementExists));

											   if (!elementExists)
											   {
												   LogElemNotFound();
												   return false;
											   }


											   bool result;
											   switch (targetElement.TagName.ToLower())
											   {
												   case "input":
												   case "a":
												   case "img":
												   case "select":
												   case "option":
												   case "button":
												   case "tr":
												   case "td":
													   {
														   Log.Info(string.Format("Stale test for {0}", webElement.Identifier));
														   var displayed = targetElement.Displayed;
														   bool enabled;
														   var disabledAttributeValue = targetElement.GetAttribute("disabled");
														   if (!String.IsNullOrEmpty(disabledAttributeValue) && (disabledAttributeValue.Equals("true")))
															   enabled = false;
														   else
															   enabled= targetElement.Enabled;
														   Log.Info(string.Format("Stale test - displayed:{0}, enabled:{1}", displayed, enabled));
														   result = displayed && enabled;
													   }
													   break;
												   default:
													   {
														   // stale test for non-buttons
														   Log.Info("Stale test for non-clickable element.");
														   targetElement.Click();
														   result = true;
													   }
													   break;
											   }

											   Log.Info(String.Format("{0} -> stale test for {1} was {2}!", actionName, webElement.Identifier, result ? "successful" : "not successful"));

											   return result;
										   });
		}

		public static Func<IWebDriver, bool> ExpectedConditions_IsVisibleAndEnabled(WebElement webElement)
		{
            return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_IsVisibleAndEnabled",
									   (seleniumDriver, elementLocator, actionName) =>
									   {
											IWebElement targetElement = driver.PeekElement(webElement);

											var elementExists = targetElement != null;

                                            Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, elementExists));

											if (!elementExists)
											{
												LogElemNotFound();
												return false;
											}

                                            Log.Info(string.Format("VisibleEnabled test for {0}", webElement.Identifier));
											var displayed = targetElement.Displayed;
											bool enabled;
											var disabledAttributeValue = targetElement.GetAttribute("disabled");
											if (!String.IsNullOrEmpty(disabledAttributeValue) && (disabledAttributeValue.Equals("true")))
												enabled = false;
											else
												enabled = targetElement.Enabled;

											Log.Info(string.Format("VisibleEnabled test - displayed:{0}, enabled:{1}", displayed, enabled));
											var result = displayed && enabled;

                                            Log.Info(String.Format("{0} -> visibleEnabled test for {1} was {2}!", actionName, webElement.Identifier, result ? "successful" : "not successful"));

											return result;
										});
		}


		public static Func<IWebDriver, bool> ExpectedConditions_ElementIsEnabled(WebElement webElement)
		{

            return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_ElementIsExists",
									   (seleniumDriver, elementLocator, actionName) =>
									   {
											IWebElement targetElement = driver.PeekElement(webElement);

											var elementDoesNotExist =  targetElement == null;

											if (elementDoesNotExist)
											{
												LogElemNotFound();
												return false;
											}

											Log.Info(string.Format("Chek if enabled for : {0}", targetElement.TagName));
											var disabledAttributeValue = targetElement.GetAttribute("disabled");
											if (!String.IsNullOrEmpty(disabledAttributeValue) && (disabledAttributeValue.Equals("true")))
											   return false;
											bool enabled = targetElement.Enabled;
                                            Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, enabled));
											return enabled;             
									   });
		}
		public static Func<IWebDriver, bool> ExpectedConditions_ElementIsNotEnabled(WebElement webElement)
		{

            return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_ElementIsExists",
									   (seleniumDriver, elementLocator, actionName) =>
									   {
											IWebElement targetElement = driver.PeekElement(webElement);

											var elementDoesNotExist = targetElement == null;

											if (elementDoesNotExist)
											{
												LogElemNotFound();
												return false;
											}

                                            Log.Info(string.Format("Chek if enabled for : {0}", webElement.Identifier));
											var disabledAttributeValue = targetElement.GetAttribute("disabled");
											if (!String.IsNullOrEmpty(disabledAttributeValue) && (disabledAttributeValue.Equals("true")))
												return true;
											bool notEnabled = !targetElement.Enabled;
                                            Log.Info(string.Format("{0} {1} = {2}", actionName, webElement.Identifier, notEnabled));
											return notEnabled;
									   });
		}

		public static Func<IWebDriver, bool> ExpectedConditions_TextPresent(WebElement webElement, string text)
		{
            return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_TextPresent",
									   (seleniumDriver, elementLocator, actionName) =>
									   {
											IWebElement targetElement = driver.PeekElement(webElement);

											var elementDoesNotExist = targetElement == null;

											if (elementDoesNotExist)
											{
												LogElemNotFound();
												return false;
											}

											string elementValue = targetElement.GetAttribute("value") ?? targetElement.Text ?? string.Empty;
											bool textIsPresent = elementValue.ToLowerInvariant().Contains(text.ToLowerInvariant());

                                            Log.Info(string.Format("{0} -> {1} value {2} contains {3} = {4}", actionName, webElement.Identifier, elementValue, text, textIsPresent));

											return textIsPresent;
									   });
		}

		public static Func<IWebDriver, bool> ExpectedConditions_TextNotPresent(WebElement webElement, string text)
		{
            return driver => TryAction(driver, webElement.Identifier, "ExpectedConditions_TextPresent",
									   (seleniumDriver, elementLocator, actionName) =>
										   {
												IWebElement targetElement = driver.PeekElement(webElement);

												var elementDoesNotExist = targetElement == null;

												if (elementDoesNotExist)
												{
													LogElemNotFound();
													throw new ElementNotPresentException(webElement);
												}

												string elementValue = targetElement.GetAttribute("value") ??
																		targetElement.Text ?? string.Empty;
												bool textIsNotPresent =
													!elementValue.ToLowerInvariant().Contains(text.ToLowerInvariant());

												Log.Info(string.Format("{0} -> {1} value {2} contains {3} = {4}",
                                                                        actionName, webElement.Identifier, elementValue, text,
																		textIsNotPresent));

												return textIsNotPresent;
										   });
		}


		private static bool TryAction(IWebDriver driver, string identifier, string actionName, Func<IWebDriver, string, string, bool> action)
		{
			try
			{
                return action(driver, identifier, actionName);
			}
			catch (WebDriverTimeoutException wte)
			{
			}
			catch (StaleElementReferenceException sere)
			{
                Log.Info(string.Format("{0} {1} = {2}", actionName, identifier, "StaleElementReferenceException"), sere);
			}
			catch (ElementNotVisibleException)
			{
				Log.Info(
                    String.Format("ExpectedConditions_ElementNotExists -> ElementNotVisibleException exception when trying to locate : {0}", identifier));
			}
			catch (InvalidOperationException ioe)
			{
                Log.Info(string.Format("{0} {1} = {2}", actionName, identifier, "InvalidOperationException"), ioe);

			}
			catch (WebDriverException wde)
			{
                Log.Info(string.Format("ExpectedConditions_ElementNotExists {0} = {1}", identifier, "WebDriverException"), wde);
			}

			return false;
		}
	}

	public static class Until
	{
		public static WaitElementHelper Element(WebElement element)
		{
			return new WaitElementHelper(element);
		}

		public static WaitTitleHelper Title
		{
			get { return new WaitTitleHelper(Globals.Driver); }
		}
	}   
}
