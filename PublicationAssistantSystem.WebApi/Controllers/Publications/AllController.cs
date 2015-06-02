﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Serialization;
using AutoMapper;
using PublicationAssistantSystem.Core.Infrastructure;
using PublicationAssistantSystem.DAL.Context;
using PublicationAssistantSystem.DAL.DTO.Publications;
using PublicationAssistantSystem.DAL.Models.Misc;
using PublicationAssistantSystem.DAL.Models.Publications;
using PublicationAssistantSystem.DAL.Repositories.Specific.Interfaces;

namespace PublicationAssistantSystem.WebApi.Controllers.Publications
{
    /// <summary>
    /// Provides access to publications repository.
    /// </summary>
    [RoutePrefix("api/Publications/All")]
    public class AllController : ApiController
    {
        private readonly IPublicationAssistantContext _db;
        private readonly IPublicationBaseRepository _publicationBaseRepository;
        private readonly IEmployeeRepository _employeeRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="db"> Db context. </param>
        /// <param name="publicationBaseRepository"> Repository of publications. </param>
        /// <param name="employeeRepository"> Repository of employees. </param>
        public AllController(
            IPublicationAssistantContext db, 
            IPublicationBaseRepository publicationBaseRepository, IEmployeeRepository employeeRepository)
        {
            _db = db;
            _publicationBaseRepository = publicationBaseRepository;
            _employeeRepository = employeeRepository;
        }
        
        /// <summary>
        /// Returns all publications.
        /// </summary>
        /// <remarks> GET: api/Publications/All </remarks>
        /// <returns> All publications. </returns>
        [Route("")]
        public IEnumerable<PublicationBaseDTO> GetAll()
        {
            var publications = _publicationBaseRepository.Get();

            var mapped = publications.Select(Mapper.Map<PublicationBaseDTO>).ToList();
            return mapped;
        }

        /// <summary>
        /// Returns publication with given id.
        /// </summary>
        /// <param name="publicationId"> Publication id. </param>
        /// /// <remarks> GET: api/Publications/All/Id </remarks>
        /// <returns> Publication with specified id. </returns>
        [Route("{publicationId:int}")]
        public PublicationBaseDTO GetPublicationById(int publicationId)
        {
            var publication = _publicationBaseRepository.Get(x => x.Id == publicationId).FirstOrDefault();
            if (publication == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var mapped = Mapper.Map<PublicationBaseDTO>(publication);
            return mapped;
        }

        /// <summary> 
        /// Gets the publications of employee with specified id.
        ///  </summary>
        /// <param name="request">Http request</param>
        /// <param name="employeeId"> Identifier of employee whose publications will be returned. </param>
        /// /// <remarks> GET: api/Employees/Id/Publications </remarks>
        /// <returns> Publications associated with specified employee. </returns>
        [Route("~/api/Employees/{employeeId:int}/Publications")]
        [ResponseType(typeof(IEnumerable<PublicationBaseDTO>))]
        public HttpResponseMessage GetPublicationsOfEmployee(HttpRequestMessage request, int employeeId)
        {
            var employee = _employeeRepository.GetByID(employeeId);
            if (employee == null)
            {
                return request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    string.Format("Not found employee with id:{0}", employeeId));
            }

            var mapped = employee.Publications.Select(Mapper.Map<PublicationBaseDTO>).ToList();
            return request.CreateResponse(mapped);
        }

        [Route("~/GetAllAsXml")]
        public HttpResponseMessage GetAllPublications()
        {
            var publications = _publicationBaseRepository.Get();
            var mapped = publications.Select(Mapper.Map<PublicationBaseDTO>).ToArray();

            var xml = new XmlSerializer(typeof(PublicationBaseDTO[]));

            using (var stream = new MemoryStream())
            {
                xml.Serialize(stream, mapped);

                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new ByteArrayContent(stream.ToArray());
                result.Content.Headers.ContentType = 
                    new MediaTypeHeaderValue("text/xml");
                result.Content.Headers.ContentDisposition = 
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = String.Format("DumpAll_{0}.xml", DateTime.Now.ToString("yyyMMddHHmmss")),
                    };
                return result;
            }
        }

        /// <summary>
        /// Deletes the publication with given id.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="publicationId"> The id of publication to delete. </param>
        /// <remarks> DELETE api/Publications/Id </remarks>
        [HttpDelete]
        [Route("~/api/Publications/{publicationId:int}")]
        public void Delete(int publicationId)
        {
            _publicationBaseRepository.Delete(publicationId);
            _db.SaveChanges();
        }
    }
}