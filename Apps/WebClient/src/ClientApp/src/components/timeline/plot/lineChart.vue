<script lang="ts">
import { Vue, Component, Prop, Watch } from "vue-property-decorator";
import { Line, mixins } from "vue-chartjs";

@Component({
    extends: Line,
    mixins: [mixins.reactiveData],
})
export default class LineChartComponent extends Vue<Line> {
    @Prop() chartdata: any;
    @Prop() options: any;

    constructor() {
        super();
    }

    private mounted() {
        this.renderChart(this.chartdata, this.options);
    }
}
</script>
