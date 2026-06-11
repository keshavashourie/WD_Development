using System;
using System.Web;
using System.Threading.Tasks;
using Camstar.WebPortal.FormsFramework.Utilities;
using System.IO;
using Siemens.PLM.SDK.DSS.API;
using Siemens.PLM.SDK.DSS.Model;

/// <summary>
/// Namespace responsible for communication with XceleratorShare APIs
/// </summary>
namespace Camstar.WebPortal.WebPortlets.XceleratorShare
{
    /// <summary>
    /// Summary description for DSSClient
    /// </summary>
    public class DSSClient
    {
        private IDssClient dssClient;
        private const string _strContentType = "content type";
        public DSSClient(string accessKey, string secretAccessKey)
        {
            try
            {
                var ClientConfig = new DssClientConfiguration()
                {
                    ACCESS_KEY_ID = accessKey,
                    SECRET_ACCESS_KEY = secretAccessKey,
                    DSS = LCSSetting.DSS
                };
                dssClient = DssClientFactory.GetDSSClient(ClientConfig);
            }
            catch (Exception ex)
            {

                dssClient = null;
                throw new DSSClientException(ErrorMessage.DSSClientConnectionError);
            }

        }

        public void UploadDocumentAsync(string ticketId, string filePath, string fileName)
        {
            try
            {
                Siemens.PLM.SDK.DSS.Model.UploadFileByTicketRequest request = new Siemens.PLM.SDK.DSS.Model.UploadFileByTicketRequest
                {
                    filePath = filePath,
                    uploadFileInput = new Siemens.PLM.SDK.DSS.Model.UploadFileInput()
                    {
                        filename = fileName
                    }
                };
                var taskStat = Task.Run(async () => await dssClient.UploadFileByTicketAsync(ticketId, request));
                taskStat.Wait();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(_strContentType))
                {
                    throw new DSSClientException(ErrorMessage.DSSClientInvalidContentError + "-" + ex.Message.Split(',')[2].ToString());
                }
                else
                {
                    throw ex;
                }
            }
        }

        public async void DownloadFileByTicket(HttpResponse response, string ticketId)
        {
            Stream responseStream = response.OutputStream;
            int chunkSize = 5 * 1024 * 1024; // 5 MB
            Siemens.PLM.SDK.DSS.Model.DownloadInfo downloadInfo = new DownloadInfo { nBytes = chunkSize };
            DownloadFileByTicketRequest request = new DownloadFileByTicketRequest { ticketId = ticketId, downloadInfo = downloadInfo };

            int len = downloadInfo.nBytes;
            bool areHeadersSet = false;
            try
            {
                do
                {
                    DownloadFileResponse downloadResponse = dssClient.DownloadFileByTicket(request);
                    if (!areHeadersSet)
                    {
                        areHeadersSet = true;
                        response.AddHeader("Content-Disposition", $"attachment; filename=\"{downloadResponse.filename}\"");
                        response.ContentType = downloadResponse.streamContent.Headers.ContentType.MediaType;
                        string fileLength = downloadResponse.fileLength.ToString();
                        response.AddHeader("Content-Length", fileLength);
                    }
                    using (Stream input = await downloadResponse.streamContent.ReadAsStreamAsync())
                    {
                        int iRead = 0;
                        byte[] bytes = new byte[downloadInfo.nBytes];
                        while ((iRead = input.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            responseStream.Write(bytes, 0, iRead);
                            responseStream.Flush();
                        }
                        len = (int)input.Length;
                    }
                    downloadInfo.index++;

                } while (len == downloadInfo.nBytes);
            }
            catch (Exception exception)
            {
                throw new Exception(ErrorMessage.DownloadFailure);
            }
        }

        public string GetDesignTypeName(string fileExt)
        {
            try
            {
                return MCADExtensions.elements.Find(x => x.fileExtensions.Contains(fileExt))?.name;
            }
            catch (Exception exception)
            {
                HttpContext.Current.Trace.Write(exception.ToString());
                return "";
            }
        }
    }
}