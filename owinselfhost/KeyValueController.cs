﻿using Microsoft.Owin.Logging;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace owinselfhost;
public class ValuesController : ApiController
{
    public ValuesController()
    {
    }
    // GET api/values 
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2 ==> " + DateTime.Now.ToLongDateString() };
    }

    // GET api/values/5 
    public string Get(int id)
    {
        return "value";
    }

    // POST api/values 
    public void Post([FromBody] string value)
    {
    }

    // PUT api/values/5 
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/values/5 
    public void Delete(int id)
    {
    }
}
