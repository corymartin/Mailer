Mailer
======

Simple C# email library with templating support.

Uses [Mustache](http://mustache.github.com/) templates via [Nustache](https://github.com/jdiamond/Nustache).


API
---

```csharp
Mailer.Send(string to, string template, dynamic templateData, params System.Net.Mail.Attachment[] attachments)
```

Example:

Assume templates root is `C:\example\templates\` using the example template in the repo.

```csharp
Mailer.Send("jim@example.com", @"password\update", new {
    firstname = "Jim",
    passwordtoken = "askf238fuhawf2983ghf"
});
```

If a `*.html` template is present, an html email will be sent.

If a `*.txt` template is present, a text email will be sent.

If both are present, a multipart email will be sent.

The `*.cfg` file has two fields, `from` and `subject`.
Both are parsed as Mustache templates.

- - - - -

```csharp
Mailer.Send(string to, string from, string subject, string body, params System.Net.Mail.Attachment[] attachments)
```
