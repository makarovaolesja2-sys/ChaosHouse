using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ChaosHouse.Core.Persistence.DTOs;

namespace ChaosHouse.Core.Persistence
{
    public class XmlWorldStorage : IWorldStorage
    {
        public async Task SaveAsync(
            WorldDto world,
            string path,
            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(world);

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Faili tee ei tohi olla tühi.", nameof(path));

            token.ThrowIfCancellationRequested();

            var root = new XElement("World");

            var members = new XElement("Members");

            foreach (HouseMemberDto member in world.Members)
            {
                XElement element = member switch
                {
                    GhostDto ghost => new XElement("Ghost",
                        new XAttribute("Id", ghost.Id),
                        new XAttribute("Name", ghost.Name),
                        new XAttribute("TypeName", ghost.TypeName),
                        new XAttribute("Energy", ghost.Energy),
                        new XElement("SpookinessLevel", ghost.SpookinessLevel)),

                    InventorDto inventor => new XElement("Inventor",
                        new XAttribute("Id", inventor.Id),
                        new XAttribute("Name", inventor.Name),
                        new XAttribute("TypeName", inventor.TypeName),
                        new XAttribute("Energy", inventor.Energy),
                        new XElement("CurrentGadget", inventor.CurrentGadget)),

                    TalkingFridgeDto fridge => new XElement("TalkingFridge",
                        new XAttribute("Id", fridge.Id),
                        new XAttribute("Name", fridge.Name),
                        new XAttribute("TypeName", fridge.TypeName),
                        new XAttribute("Energy", fridge.Energy),
                        new XElement("IsLocked", fridge.IsLocked)),

                    _ => throw new InvalidOperationException(
                        "Tundmatu tegelase tüüp.")
                };

                members.Add(element);
            }

            var groups = new XElement("Groups");

            foreach (GroupDto group in world.Groups)
            {
                var groupElement = new XElement("Group",
                    new XAttribute("Id", group.Id),
                    new XAttribute("Name", group.Name));

                foreach (string memberId in group.MemberIds)
                    groupElement.Add(new XElement("MemberId", memberId));

                groups.Add(groupElement);
            }

            root.Add(members);
            root.Add(groups);

            var document = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                root);

            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                4096,
                FileOptions.Asynchronous);

            await document.SaveAsync(stream, SaveOptions.None, token);
        }

        public async Task<WorldDto> LoadAsync(
            string path,
            CancellationToken token = default)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(
                    "Faili tee ei tohi olla tühi.", nameof(path));

            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous);

            XDocument document = await XDocument.LoadAsync(
                stream,
                LoadOptions.None,
                token);

            XElement root = document.Element("World")
                ?? throw new InvalidDataException(
                    "XML-failis puudub World element.");

            var world = new WorldDto();

            XElement members = root.Element("Members")
                ?? throw new InvalidDataException(
                    "XML-failis puudub Members element.");

            foreach (XElement element in members.Elements())
            {
                string id = (string?)element.Attribute("Id")
                    ?? throw new InvalidDataException("Tegelase ID puudub.");

                string name = (string?)element.Attribute("Name")
                    ?? throw new InvalidDataException("Tegelase nimi puudub.");

                string typeName = (string?)element.Attribute("TypeName")
                    ?? throw new InvalidDataException("Tegelase tüüp puudub.");

                int energy = (int?)element.Attribute("Energy")
                    ?? throw new InvalidDataException("Tegelase energia puudub.");

                HouseMemberDto dto = element.Name.LocalName switch
                {
                    "Ghost" => new GhostDto
                    {
                        Id = id,
                        Name = name,
                        TypeName = typeName,
                        Energy = energy,
                        SpookinessLevel = (int?)element.Element("SpookinessLevel")
                            ?? throw new InvalidDataException("SpookinessLevel puudub.")
                    },

                    "Inventor" => new InventorDto
                    {
                        Id = id,
                        Name = name,
                        TypeName = typeName,
                        Energy = energy,
                        CurrentGadget = (string?)element.Element("CurrentGadget")
                            ?? throw new InvalidDataException("CurrentGadget puudub.")
                    },

                    "TalkingFridge" => new TalkingFridgeDto
                    {
                        Id = id,
                        Name = name,
                        TypeName = typeName,
                        Energy = energy,
                        IsLocked = (bool?)element.Element("IsLocked")
                            ?? throw new InvalidDataException("IsLocked puudub.")
                    },

                    _ => throw new InvalidDataException(
                        $"Tundmatu tegelase tüüp: {element.Name}")
                };

                world.Members.Add(dto);
            }

            XElement groups = root.Element("Groups")
                ?? throw new InvalidDataException(
                    "XML-failis puudub Groups element.");

            foreach (XElement element in groups.Elements("Group"))
            {
                var group = new GroupDto
                {
                    Id = (string?)element.Attribute("Id")
                        ?? throw new InvalidDataException("Grupi ID puudub."),

                    Name = (string?)element.Attribute("Name")
                        ?? throw new InvalidDataException("Grupi nimi puudub.")
                };

                foreach (XElement memberId in element.Elements("MemberId"))
                    group.MemberIds.Add(memberId.Value);

                world.Groups.Add(group);
            }

            return world;
        }
    }
}
