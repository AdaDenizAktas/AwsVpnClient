using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using Amazon;
using AwsVpnClient.Core.Interfaces;

namespace AwsVpnClient.Infrastructure.Aws
{
    public class AwsVpnProvisioner : IAwsVpnProvisioner
    {
        private BasicAWSCredentials _credentials;

        public void SetCredentials(string accessKey, string secret)
        {
            _credentials = new BasicAWSCredentials(accessKey, secret);
        }

        public async Task<List<string>> GetRegionsAsync()
        {
            var config = new AmazonEC2Config
            {
                RegionEndpoint = RegionEndpoint.USEast1 // Required dummy region to query all regions
            };

            using var ec2 = new AmazonEC2Client(_credentials, config);
            var result = await ec2.DescribeRegionsAsync();
            return result.Regions.Select(r => r.RegionName).ToList();
        }

        public async Task<string> CreateVpnServerAsync(string region)
        {
            var client = new AmazonEC2Client(_credentials, Amazon.RegionEndpoint.GetBySystemName(region));

            // Dynamically resolve path to install.sh in output directory
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "WireGuardScript", "install.sh");
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException("install.sh not found at: " + scriptPath);

            // Lookup latest Ubuntu 20.04 AMI for the region
            var describeImagesRequest = new DescribeImagesRequest
            {
                Owners = new List<string> { "099720109477" }, // Canonical (official Ubuntu AMI owner)
                Filters = new List<Filter>
        {
            new Filter("name", new List<string> { "ubuntu/images/hvm-ssd/ubuntu-focal-20.04-amd64-server-*" }),
            new Filter("architecture", new List<string> { "x86_64" }),
            new Filter("root-device-type", new List<string> { "ebs" }),
            new Filter("virtualization-type", new List<string> { "hvm" })
        }
            };

            var imagesResponse = await client.DescribeImagesAsync(describeImagesRequest);
            var latestImage = imagesResponse.Images.OrderByDescending(i => i.CreationDate).FirstOrDefault();

            if (latestImage == null)
                throw new Exception("No suitable Ubuntu 20.04 image found.");

            // Create EC2 instance
            var runRequest = new RunInstancesRequest
            {
                ImageId = latestImage.ImageId,
                InstanceType = InstanceType.T2Micro,
                MinCount = 1,
                MaxCount = 1,
                UserData = Convert.ToBase64String(File.ReadAllBytes(scriptPath))
            };

            var runResponse = await client.RunInstancesAsync(runRequest);
            var instanceId = runResponse.Reservation.Instances.First().InstanceId;

            await client.CreateTagsAsync(new CreateTagsRequest
            {
                Resources = new List<string> { instanceId },
                Tags = new List<Tag> { new Tag("Name", "WireGuard-VPN") }
            });

            // Poll until public IP is assigned
            DescribeInstancesResponse describeResponse;
            string publicIp;
            do
            {
                await Task.Delay(5000);
                describeResponse = await client.DescribeInstancesAsync(new DescribeInstancesRequest
                {
                    InstanceIds = new List<string> { instanceId }
                });
                publicIp = describeResponse.Reservations[0].Instances[0].PublicIpAddress;
            } while (string.IsNullOrEmpty(publicIp));

            return publicIp;
        }
        public async Task TerminateVpnServerAsync(string region)
        {
            var client = new AmazonEC2Client(_credentials, Amazon.RegionEndpoint.GetBySystemName(region));
            var instances = await client.DescribeInstancesAsync();

            var idsToTerminate = instances.Reservations
                .SelectMany(r => r.Instances)
                .Where(i => i.Tags.Any(t => t.Key == "Name" && t.Value == "WireGuard-VPN"))
                .Select(i => i.InstanceId)
                .ToList();

            if (idsToTerminate.Count > 0)
            {
                await client.TerminateInstancesAsync(new TerminateInstancesRequest
                {
                    InstanceIds = idsToTerminate
                });
            }
        }
        public async Task<string?> GetActiveRegionAsync(List<string> allRegions)
        {
            foreach (var regionName in allRegions)
            {
                var client = new AmazonEC2Client(_credentials, RegionEndpoint.GetBySystemName(regionName));

                var instances = await client.DescribeInstancesAsync(new DescribeInstancesRequest());
                var active = instances.Reservations
                    .SelectMany(r => r.Instances)
                    .FirstOrDefault(i =>
                        i.State.Name == InstanceStateName.Running &&
                        i.Tags.Any(t => t.Key == "Name" && t.Value == "WireGuard-VPN"));

                if (active != null)
                    return regionName;
            }

            return null;
        }



    }
}
