using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;

public class ContactBook
{
    const string BaseURL = "https://contactbook.nakov.repl.co/api";
    RestClient client;

    [SetUp]
    public void Setup()
    {
        client = new RestClient(BaseURL);
    }

    [Test]
    public void Test_ContactBook_ListContacts()
    {
        var request = new RestRequest("/contacts", Method.GET);
        var response = client.Execute(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        List<Contact> contacts =
            JsonSerializer.Deserialize<List<Contact>>(response.Content);
        Assert.That(contacts.Count > 0);
        Assert.AreEqual("Steve", contacts[0].firstName);
        Assert.AreEqual("Jobs", contacts[0].lastName);
    }

    [Test]
    public void Test_ContactBook_FindExistingContact()
    {
        var request = new RestRequest("/contacts/search/{keyword}", Method.GET);
        request.AddUrlSegment("keyword", "albert");
        var response = client.Execute(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        List<Contact> contacts =
            JsonSerializer.Deserialize<List<Contact>>(response.Content);
        Assert.That(contacts.Count > 0);
        Assert.AreEqual("Albert", contacts[0].firstName);
        Assert.AreEqual("Einstein", contacts[0].lastName);
    }

    [Test]
    public void Test_ContactBook_FindNonExistingContact()
    {
        var request = new RestRequest("/contacts/search/{keyword}", Method.GET);
        request.AddUrlSegment("keyword", "keyword" + DateTime.Now.Ticks);
        var response = client.Execute(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        List<Contact> contacts =
            JsonSerializer.Deserialize<List<Contact>>(response.Content);
        Assert.AreEqual(0, contacts.Count);
    }

    [Test]
    public void Test_ContactBook_CreateContactInvalidData()
    {
        var request = new RestRequest("/contacts", Method.POST);
        request.AddJsonBody(new
        {
            firstname = "Someone"
        });
        var response = client.Execute(request);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    public void Test_ContactBook_CreateContactValidData()
    {
        // Add new contact
        var request = new RestRequest("/contacts", Method.POST);
        var newContact = new
        {
            firstName = "fname" + DateTime.Now.Ticks,
            lastName = "lname" + DateTime.Now.Ticks,
            email = "email" + DateTime.Now.Ticks + "@abv.bg",
            phone = "+359 " + DateTime.Now.Ticks,
            comments = "some comments"
        };
        request.AddJsonBody(newContact);
        var response = client.Execute(request);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        // Find the contact and assert it is correctly added
        var requestContacts = new RestRequest("/contacts", Method.GET);
        var responseContacts = client.Execute(requestContacts);
        Assert.AreEqual(HttpStatusCode.OK, responseContacts.StatusCode);
        List<Contact> contacts =
            JsonSerializer.Deserialize<List<Contact>>(responseContacts.Content);
        var lastContact = contacts.Last();
        Assert.AreEqual(newContact.firstName, lastContact.firstName);
        Assert.AreEqual(newContact.lastName, lastContact.lastName);
        Assert.AreEqual(newContact.email, lastContact.email);
        Assert.AreEqual(newContact.phone, lastContact.phone);
        Assert.AreEqual(newContact.comments, lastContact.comments);
    }
}