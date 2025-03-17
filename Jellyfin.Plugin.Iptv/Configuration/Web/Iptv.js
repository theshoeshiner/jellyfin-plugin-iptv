


const url = (name) =>
    ApiClient.getUrl("web/ConfigurationPage", {
        name: name,
    })
    /*ApiClient.getUrl("configurationpage", {
        name,
    })*/;


$(document).ready(() => {
    console.log("adding link");
    const style = document.createElement('link');
    style.rel = 'stylesheet';
    style.href = url('Iptv.css')
    style.type = "text/css";
    console.log(style);
    document.head.appendChild(style);
});

const pluginConfig = {
    UniqueId: '4e447dd6-5725-44a5-ac9c-d23e23f6343e'
};

// Returns a Promise
const fetchJson = (url) => {
    console.log("fetchJson: " + url);
    return ApiClient.fetch({
        dataType: 'json',
        type: 'GET',
        url: ApiClient.getUrl(url),
    })
};

const channelFilter = {
    name: null,
    category: null,
    country: null,
    subdivision: null,
    network: null,
    language: null
};

const updateChannelFilter = (view) => {

    currentPage = 0;

    //let filters = view.querySelectorAll("#channels-table thead th input");
    let thead = view.querySelector("#channels-table thead");

    console.log(view);
    console.log("thead");
    console.log(thead);

    let addedFilter = thead.querySelector("th.added input");
    let streamFilter = thead.querySelector("th.stream input");
    let guideFilter = thead.querySelector("th.guide input");
    let networkFilter = thead.querySelector("th.network input");
    let nameFilter = thead.querySelector("th.name input");
    let categoryFilter = thead.querySelector("th.categories input");
    let countryFilter = thead.querySelector("th.country input");
    let subdivisionFilter = thead.querySelector("th.region input");
    let languageFilter = thead.querySelector("th.languages input");

    console.log(addedFilter);
    console.log(streamFilter);
    console.log(nameFilter);
    console.log(networkFilter);
    console.log(categoryFilter);

    channelFilter.added = addedFilter.checked;
    channelFilter.guide = guideFilter.checked;
    channelFilter.name = nameFilter.value.length > 0 ? nameFilter.value.toLowerCase() : null;
    channelFilter.network = networkFilter.value.length > 0 ? networkFilter.value.toLowerCase() : null;
    channelFilter.category = categoryFilter.value.length > 0 ? categoryFilter.value.toLowerCase() : null;
    channelFilter.country = countryFilter.value.length > 0 ? countryFilter.value.toLowerCase() : null;
    channelFilter.language = languageFilter.value.length > 0 ? languageFilter.value.toLowerCase() : null;
    channelFilter.subdivision = subdivisionFilter.value.length > 0 ? subdivisionFilter.value.toLowerCase() : null;
    channelFilter.stream = streamFilter.checked;
    populateChannelsTable(view);
};

const toggleChannel = (td, channel) => {
    let id = channel.Id;
    let selected = selectedChannels.channels.includes(id);
    if (selected) {
        fetchJson("Iptv/UnselectChannel/" + id);
        selectedChannels.channels.splice(selectedChannels.channels.indexOf(id), 1)
        td.innerHTML = "<button class='emby-button raised block button-submit' ><span>Add</span></button>";
    }
    else {
        fetchJson("Iptv/SelectChannel/" + channel.Id);
        selectedChannels.channels.push(id)
        td.innerHTML = "<button class='emby-button raised block' ><span>Remove</span></button>";
    }
};

const createChannelRow = (channel) => {

    const tr = document.createElement('tr');

    tr.dataset['channelId'] = channel.Id;

    let commonClasses = "detailTableBodyCell ";
    let td = null;

    td = document.createElement('td');
    td.className = commonClasses+"added";
    if (selectedChannels.channels.includes(channel.Id)) {
        td.innerHTML = "<button class='emby-button raised block' ><span>Remove</span></button>";
    }
    else {
        td.innerHTML = "<button class='emby-button raised block button-submit' ><span>Add</span></button>";
    }
    let buttonTd = td;
    td.addEventListener('click', (e) => {
        toggleChannel(buttonTd, channel);
    });
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.HasStream ? "<div><div class='yes'>Y<div></div>" : "";
    td.className = commonClasses+"stream";
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.HasGuide ? "<div><div class='yes'>Y<div></div>" : "";
    td.className = commonClasses + "guide";
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.Network === undefined ? '' : channel.Network;
    td.title = channel.Network === undefined ? null : channel.Network
    td.className = commonClasses+"network";
    tr.appendChild(td);

    td = document.createElement('td');
    td.title = channel.Name;
    td.innerHTML = channel.Name;
    td.className = commonClasses+"name";
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.Categories.map(e => e).join(", ");
    td.className = commonClasses+"categories";
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.Country;
    td.className = commonClasses+"country";
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.Subdivision === undefined ? '' : channel.Subdivision;
    td.className = commonClasses + "subdivision";
    tr.appendChild(td);

    td = document.createElement('td');
    td.innerHTML = channel.Languages.map(e => e).join(", ");
    td.className = commonClasses + "languages";
    tr.appendChild(td);


    return tr;
};

const allChannels = {};

const selectedChannels = {};

const pageSize = 10;
var currentPage = 0;

const nextPage = (view) => {
    currentPage++;
    populateChannelsTable(view);
};

const prevPage = (view) => {
    currentPage--;
    populateChannelsTable(view);
};

const populateChannelsTable = (view) => {
    let table = view.querySelector('#channels-table-body');
    let details = view.querySelector('#page-details');
    let next = view.querySelector('#next-page');
    let prev = view.querySelector('#prev-page');
    let fetchPromise = getFilteredChannels();
    fetchPromise.then((filteredChannels) => {
        table.innerHTML = "";
        //let pages = filteredChannels.length / pageSize;
        let currentPageStart = currentPage * pageSize;
        let currentPageEnd = currentPageStart + pageSize;
        console.log("currentPageStart: " + currentPageStart);
        let size = 0;
        for (let i = currentPageStart; i < filteredChannels.length && i < currentPageEnd; i++) {
            const channel = filteredChannels[i];
            const elem = createChannelRow(channel);
            table.appendChild(elem);
            size++;
        }
        details.innerHTML = (currentPageStart + 1) + " - " + (currentPageStart + size) + " of " + filteredChannels.length;
        prev.disabled = currentPageStart == 0;
        next.disabled = currentPageStart + size >= filteredChannels.length;
    });
};


const filter = (value, check) => {
    return check == null  || (value != null && value.toLowerCase().includes(check));
};

const getFilteredChannels = () => {
    console.log("getFilteredChannels filter:");
    console.log(channelFilter);
    let allChannelsPromise = getAllChannels();
    allChannelsPromise = allChannelsPromise.then(() => {
        console.log("filtering channels...");
        let filteredChannels = [];
        for (let i = 0; i < allChannels.channels.length; i++) {
            let channel = allChannels.channels[i];
            if (
                (!channelFilter.added || selectedChannels.channels.includes(channel.Id)) &&
                filter(channel.Name, channelFilter.name) &&
                filter(channel.Country, channelFilter.country) &&
                filter(channel.Network, channelFilter.network) &&
                filter(channel.categoriesString, channelFilter.category) &&
                filter(channel.Subdivision, channelFilter.subdivision) &&
                filter(channel.languagesString, channelFilter.language) &&
                (!channelFilter.stream || channel.HasStream) &&
                (!channelFilter.guide || channel.HasGuide)
                ) {
                filteredChannels.push(channel);
            }
        }
        console.log(filteredChannels.length + " filtered channels");
        return filteredChannels;
    });
    return allChannelsPromise;
};

const getAllChannels = () => {

    console.log("getAllChannels");


    let fetchPromise = Promise.resolve(allChannels.channels);

    if (allChannels.channels == null) {

        console.log("channels was null")
        Dashboard.showLoadingMsg();

        let selectedPromise = fetchJson('Iptv/SelectedChannels').then((data) => selectedChannels.channels = data);

        fetchPromise = fetchJson('Iptv/AllChannels');

        console.log("fetchPromise: " + fetchPromise);

        fetchPromise = fetchPromise.then((data) => {
            allChannels.channels = data;
            console.log("set allChannels with " + data.length);
            for (let i = 0; i < data.length; i++) {
                let channel = data[i];
                channel.categoriesString = channel.Categories.map(e => e).join(" ");
                channel.languagesString = channel.Languages.map(e => e).join(" ");
            }
            Dashboard.hideLoadingMsg();
            return data;
        });

        fetchPromise = Promise.all([fetchPromise, selectedPromise]);

    }
    //else {
      //  fetchChannels = () => allChannels;
    //}
    //console.log("fetchChannels: " + fetchChannels)

    //const fetch = fetchChannels();
    //const ret = fetchChannels();

    //console.log("fetchPromise:")
    //console.log(fetchPromise);

    return fetchPromise;

    //return fetchPromise;

    /*return Promise.all([fetch])
        .then(() => {
            console.log("returning " + allChannels.channels.length+" channels")
            return allChannels.channels;
        })
        ;*/
}

const tab = (name) => '/configurationpage?name=' + name + '.html';

const tabs = [
    {
        href: tab('IptvChannels'),
        name: 'Select Channels'
    },
    {
        href: tab('IptvChannelsList'),
        name: 'List Channels'
    },

];


const setTabs = (index) => {
    const name = tabs[index].name;
    LibraryMenu.setTabs(name, index, () => tabs);
}


export default {
    fetchJson,
    pluginConfig,
    populateChannelsTable,
    updateChannelFilter,
    setTabs,
    nextPage,
    prevPage
}
