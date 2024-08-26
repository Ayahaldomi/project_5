var simplemaps_countrymap_mapdata={
  main_settings: {
   //General settings
    width: "responsive", //'700' or 'responsive'
    background_color: "#FFFFFF",
    background_transparent: "yes",
    border_color: "#ffffff",
    
    //State defaults
    state_description: "State description",
    state_color: "#88A4BC",
    state_hover_color: "#3B729F",
    state_url: "",
    border_size: 1.5,
    all_states_inactive: "no",
    all_states_zoomable: "yes",
    
    //Location defaults
    location_description: "Location description",
    location_url: "",
    location_color: "#FF0067",
    location_opacity: 0.8,
    location_hover_opacity: 1,
    location_size: 25,
    location_type: "square",
    location_image_source: "frog.png",
    location_border_color: "#FFFFFF",
    location_border: 2,
    location_hover_border: 2.5,
    all_locations_inactive: "no",
    all_locations_hidden: "no",
    
    //Label defaults
    label_color: "#ffffff",
    label_hover_color: "#ffffff",
    label_size: 16,
    label_font: "Arial",
    label_display: "auto",
    label_scale: "yes",
    hide_labels: "no",
    hide_eastern_labels: "no",
   
    //Zoom settings
    zoom: "yes",
    manual_zoom: "yes",
    back_image: "no",
    initial_back: "no",
    initial_zoom: "-1",
    initial_zoom_solo: "no",
    region_opacity: 1,
    region_hover_opacity: 0.6,
    zoom_out_incrementally: "yes",
    zoom_percentage: 0.99,
    zoom_time: 0.5,
    
    //Popup settings
    popup_color: "white",
    popup_opacity: 0.9,
    popup_shadow: 1,
    popup_corners: 5,
    popup_font: "12px/1.5 Verdana, Arial, Helvetica, sans-serif",
    popup_nocss: "no",
    
    //Advanced settings
    div: "map",
    auto_load: "yes",
    url_new_tab: "no",
    images_directory: "default",
    fade_time: 0.1,
    link_text: "View Website",
    popups: "detect",
    state_image_url: "",
    state_image_position: "",
    location_image_url: ""
  },
  state_specific: {
    JOAJ: {
      color: "#66ff8c",
      hover_color: "#16fb00",
      name: "عجلون",
      description: "اربعة مقاعد اثنين تنافس واحد مسيحي واحد كوتا"
    },
    JOAM: {
      color: "#009926",
      hover_color: "#00bfe5",
      name: "عمان",
      description: ": اربعه تنافس واحد كوتا واحد شركسي عمان الاولى  :خمس مقاعد تنافس واحد كوتا .عمان الثانية :ستة مقاعد تنافس واحد كوتا واحد مسيحي عمان الثالثه"
    },
    JOAQ: {
      hover_color: "#cadde1",
      name: "العقبة",
      description: "مقاعد تنافس اثنين واحد مقعد كوتا",
      color: "#43c4df"
    },
    JOAT: {
      hover_color: "#b874c1",
      name: "الطفيلة",
      description: "مقاعد تنافس اثنين واحد مقعد كوتا",
      color: "#e057f2"
    },
    JOAZ: {
      hover_color: "#be6acb",
      name: "الزرقاء",
      description: "سبعه منهم تنافس واحد مسيحي واحد كوتا  و احد شركسي عشر مقاعد",
      color: "#43164a"
    },
    JOBA: {
      hover_color: "#c17964",
      name: "البلقاء",
      description: "الدائرة الانتخابية الثالثة\tلواء الهاشمية\tنائب مسلم",
      color: "#ec562a"
    },
    JOIR: {
      hover_color: "#85f0c1",
      name: "اربد",
      description: "الدائرة الانتخابية الاولى\tسبعه تنافس واحد كوتا \nالدائرة الانتخابية الثانية\tخمسة تنافس واحد كوتا واحد مسيحي",
      color: "#319c6d"
    },
    JOJA: {
      hover_color: "#e570a8",
      name: "جرش",
      color: "#c12972",
      description: "اربع مقاعد ثلاث مقاعد تنافس واحد كوتا"
    },
    JOKA: {
      hover_color: "#dccee5",
      name: "الكرك",
      color: "#890dda",
      description: "ثمن مقاعد سته تنافس واحد واحد كوتا واحد مسيحي"
    },
    JOMA: {
      hover_color: "#91d0ec",
      name: "المفرق",
      color: "#355664",
      description: "اربع مقاعد ثلاث مقاعد تنافس واحد كوتا"
    },
    JOMD: {
      hover_color: "#888b1b",
      name: "مادبا",
      color: "#dbe10f",
      description: "وواحد مسيحي اربع مقاعد اثنين مقاعد تنافس واحد كوتا"
    },
    JOMN: {
      hover_color: "#94df60",
      name: "معان",
      color: "#5b8f38",
      description: "اربع مقاعد ثلاث مقاعد تنافس واحد كوتا"
    }
  },
  locations: {},
  labels: {
    JOAJ: {
      name: "عجلون",
      parent_id: "JOAJ"
    },
    JOAM: {
      name: "عمان",
      parent_id: "JOAM"
    },
    JOAQ: {
      name: "العقبة",
      parent_id: "JOAQ"
    },
    JOAT: {
      name: "الطفيلة",
      parent_id: "JOAT"
    },
    JOAZ: {
      name: "الزرقاء",
      parent_id: "JOAZ"
    },
    JOBA: {
      name: "البلقاء",
      parent_id: "JOBA"
    },
    JOIR: {
      name: "اربد",
      parent_id: "JOIR"
    },
    JOJA: {
      name: "جرش",
      parent_id: "JOJA"
    },
    JOKA: {
      name: "الكرك",
      parent_id: "JOKA"
    },
    JOMA: {
      name: "المفرق",
      parent_id: "JOMA"
    },
    JOMD: {
      name: "مادبا",
      parent_id: "JOMD"
    },
    JOMN: {
      name: "معان",
      parent_id: "JOMN"
    }
  },
  legend: {
    entries: []
  },
  regions: {},
  data: {
    data: {
      JOAJ: "1",
      JOAM: "2"
    }
  }
};