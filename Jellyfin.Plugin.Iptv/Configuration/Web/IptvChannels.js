export default function (view) {
    view.addEventListener("viewshow", () => import(

        ApiClient.getUrl("web/ConfigurationPage", {
            name: "Iptv.js",
        }))
        .then((Iptv) => Iptv.default)
        .then((Iptv) => {
            const pluginId = Iptv.pluginConfig.UniqueId;
            Iptv.setTabs(0);


        console.log("pluginId: " + pluginId);
            Dashboard.showLoadingMsg();

        ApiClient.getPluginConfiguration(pluginId).then(function (config) {
            console.log("got config");
            console.log(config);

            //let csv = arr.map(e => JSON.stringify(e)).join(",")
            let table = view.querySelector('#channels-table-body');
            Dashboard.hideLoadingMsg();
            const getConfig = ApiClient.getPluginConfiguration(pluginId);
            Iptv.populateChannelsTable(view);

            const updateChannelFilterCall = (e) => {
                Iptv.updateChannelFilter(view);
            };

            view.querySelectorAll("#channels-table thead th input").forEach((e) => {
                e.addEventListener('change', updateChannelFilterCall)
            });

            view.querySelector('#prev-page').addEventListener("click", (e) => { Iptv.prevPage(view) });
            view.querySelector('#next-page').addEventListener("click", (e) => { Iptv.nextPage(view) });

        });




           /* view.querySelector('#name-filter').addEventListener('change', (e) => {
                console.log("name: " + e.target.value);
            });
            view.querySelector('#country-filter').addEventListener('change', (e) => {
                console.log("country: " + e.target.value);
            });
            view.querySelector('#languages-filter').addEventListener('change', (e) => {
                console.log("langugage: " + e.target.value);
            });
*/

           /* view.querySelector('#IptvChannelsForm').addEventListener('submit', (e) => {
                Dashboard.showLoadingMsg();

                ApiClient.getPluginConfiguration(pluginId).then((config) => {
                    config.ChannelFilter.Categories = view.querySelector('#Categories').value.split(",");
                    config.ChannelFilter.Countries = view.querySelector('#Countries').value.split(",");
                    config.ChannelFilter.Languages = view.querySelector('#Languages').value.split(",");
                    //config.Username = view.querySelector('#Username').value;
                    //config.Password = view.querySelector('#Password').value;
                    console.log("updating config");

                });

                e.preventDefault();
                return false;
            });*/

        }
        ));
}
