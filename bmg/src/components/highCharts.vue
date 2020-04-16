<template>
<div class="highcharts-container"></div>
</template>

<script>
import Highcharts from 'highcharts/highstock';
import Highchartsmore from 'highcharts/highcharts-more';
import HighchartsDrilldown from 'highcharts/modules/drilldown';
import Highcharts3D from 'highcharts/highcharts-3d';
Highchartsmore(Highcharts);
HighchartsDrilldown(Highcharts);
Highcharts3D(Highcharts);

export default {
  name: 'high-charts',
  props: ['option'],
  data () {
    return {
      chart: null
    };
  },
  mounted () {
    this.initChart();
  },
  methods: {
    initChart () {
      this.chart = new Highcharts.Chart(this.$el, this.option);
    }
  }
};
</script>

<style scoped>

</style>
